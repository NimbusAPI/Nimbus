using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Nimbus.ConcurrentCollections;
using Nimbus.Configuration.Settings;
using Nimbus.Extensions;
using Nimbus.Filtering.Conditions;
using Nimbus.Infrastructure;
using Nimbus.Infrastructure.Retries;
using Nimbus.MessageContracts.Exceptions;
using Nimbus.Routing;
using Nimbus.Transports.AzureServiceBus.Extensions;
using Nimbus.Transports.AzureServiceBus.Filtering;

namespace Nimbus.Transports.AzureServiceBus.QueueManagement
{
    internal class AzureQueueManager : IQueueManager
    {
        public const string DeadLetterQueuePath = "deadletteroffice";

        private readonly Func<NamespaceManager> _namespaceManager;
        private readonly Func<MessagingFactory> _messagingFactory;
        private readonly MaxDeliveryAttemptSetting _maxDeliveryAttempts;
        private readonly DefaultMessageTimeToLiveSetting _defaultMessageTimeToLive;
        private readonly AutoDeleteOnIdleSetting _autoDeleteOnIdle;
        private readonly DefaultTimeoutSetting _defaultTimeout;
        private readonly EnableDeadLetteringOnMessageExpirationSetting _enableDeadLetteringOnMessageExpiration;

        private readonly ThreadSafeLazy<ConcurrentSet<string>> _knownTopics;
        private readonly ThreadSafeLazy<ConcurrentSet<string>> _knownSubscriptions;
        private readonly ThreadSafeLazy<ConcurrentSet<string>> _knownQueues;
        private readonly ISqlFilterExpressionGenerator _sqlFilterExpressionGenerator;
        private readonly ITypeProvider _typeProvider;

        private readonly ThreadSafeDictionary<string, object> _locks = new ThreadSafeDictionary<string, object>();
        private readonly IRetry _retry;
        private readonly IPathFactory _pathFactory;

        public AzureQueueManager(Func<NamespaceManager> namespaceManager,
                                 Func<MessagingFactory> messagingFactory,
                                 AutoDeleteOnIdleSetting autoDeleteOnIdle,
                                 DefaultMessageTimeToLiveSetting defaultMessageTimeToLive,
                                 DefaultTimeoutSetting defaultTimeout,
                                 EnableDeadLetteringOnMessageExpirationSetting enableDeadLetteringOnMessageExpiration,
                                 MaxDeliveryAttemptSetting maxDeliveryAttempts,
                                 IPathFactory pathFactory,
                                 IRetry retry,
                                 ISqlFilterExpressionGenerator sqlFilterExpressionGenerator,
                                 ITypeProvider typeProvider)
        {
            _namespaceManager = namespaceManager;
            _messagingFactory = messagingFactory;
            _maxDeliveryAttempts = maxDeliveryAttempts;
            _retry = retry;
            _typeProvider = typeProvider;
            _defaultMessageTimeToLive = defaultMessageTimeToLive;
            _autoDeleteOnIdle = autoDeleteOnIdle;
            _defaultTimeout = defaultTimeout;
            _enableDeadLetteringOnMessageExpiration = enableDeadLetteringOnMessageExpiration;
            _sqlFilterExpressionGenerator = sqlFilterExpressionGenerator;
            _pathFactory = pathFactory;

            _knownTopics = new ThreadSafeLazy<ConcurrentSet<string>>(FetchExistingTopics);
            _knownSubscriptions = new ThreadSafeLazy<ConcurrentSet<string>>(FetchExistingSubscriptions);
            _knownQueues = new ThreadSafeLazy<ConcurrentSet<string>>(FetchExistingQueues);
        }

        public Task<MessageSender> CreateMessageSender(string queuePath)
        {
            return Task.Run(async () =>
                                  {
                                      EnsureQueueExists(queuePath);
                                      var messageSender = await _messagingFactory().CreateMessageSenderAsync(queuePath);
                                      return messageSender;
                                  }).ConfigureAwaitFalse();
        }

        public Task<MessageReceiver> CreateMessageReceiver(string queuePath)
        {
            return Task.Run(async () =>
                                  {
                                      EnsureQueueExists(queuePath);
                                      var receiverAsync = await _messagingFactory().CreateMessageReceiverAsync(queuePath, ReceiveMode.ReceiveAndDelete);
                                      return receiverAsync;
                                  }).ConfigureAwaitFalse();
        }

        public Task<TopicClient> CreateTopicSender(string topicPath)
        {
            return Task.Run(() =>
                            {
                                EnsureTopicExists(topicPath);

                                return _retry.Do(() =>
                                                 {
                                                     var topicClient = _messagingFactory().CreateTopicClient(topicPath);
                                                     return topicClient;
                                                 },
                                                 "Creating topic sender for " + topicPath);
                            }).ConfigureAwaitFalse();
        }

        public Task<SubscriptionClient> CreateSubscriptionReceiver(string topicPath, string subscriptionName, IFilterCondition filterCondition)
        {
            return Task.Run(() =>
                            {
                                EnsureSubscriptionExists(topicPath, subscriptionName);

                                var myOwnSubscriptionFilterCondition = new OrCondition(new MatchCondition(MessagePropertyKeys.RedeliveryToSubscriptionName, subscriptionName),
                                                                                       new IsNullCondition(MessagePropertyKeys.RedeliveryToSubscriptionName));
                                var combinedCondition = new AndCondition(filterCondition, myOwnSubscriptionFilterCondition);
                                var filterSql = _sqlFilterExpressionGenerator.GenerateFor(combinedCondition);

                                return _retry.Do(() =>
                                                 {
                                                     var subscriptionClient = _messagingFactory()
                                                         .CreateSubscriptionClient(topicPath, subscriptionName, ReceiveMode.ReceiveAndDelete);
                                                     subscriptionClient.ReplaceFilter("$Default", filterSql);
                                                     return subscriptionClient;
                                                 },
                                                 "Creating subscription receiver for topic " + topicPath + " and subscription " + subscriptionName + " with filter expression " +
                                                 filterCondition);
                            }).ConfigureAwaitFalse();
        }

        public Task MarkQueueAsNonExistent(string queuePath)
        {
            return Task.Run(() => _knownQueues.Value.Remove(queuePath)).ConfigureAwaitFalse();
        }

        public Task MarkTopicAsNonExistent(string topicPath)
        {
            return Task.Run(() => _knownTopics.Value.Remove(topicPath)).ConfigureAwaitFalse();
        }

        public Task MarkSubscriptionAsNonExistent(string topicPath, string subscriptionName)
        {
            return Task.Run(() =>
                            {
                                _knownSubscriptions.Value
                                                   .Where(path => path.StartsWith(topicPath))
                                                   .Do(key => _knownSubscriptions.Value.Remove(key))
                                                   .Done();
                            }).ConfigureAwaitFalse();
        }

        public Task<MessageSender> CreateDeadQueueMessageSender()
        {
            return CreateMessageSender(DeadLetterQueuePath);
        }

        public Task<MessageReceiver> CreateDeadQueueMessageReceiver()
        {
            return CreateMessageReceiver(DeadLetterQueuePath);
        }

        private ConcurrentSet<string> FetchExistingTopics()
        {
            return _retry.Do(() =>
                             {
                                 var topicsAsync = _namespaceManager().GetTopicsAsync();
                                 if (!topicsAsync.Wait(_defaultTimeout)) throw new TimeoutException("Fetching existing topics failed. Messaging endpoint did not respond in time.");

                                 var topics = topicsAsync.Result;
                                 var topicPaths = new ConcurrentSet<string>(topics.Select(t => t.Path));

                                 return topicPaths;
                             },
                             "Fetching existing topics");
        }

        private ConcurrentSet<string> FetchExistingSubscriptions()
        {
            return _retry.Do(() =>
                             {
                                 var subscriptionTasks = _knownTopics.Value
                                                                     .Where(WeHaveAHandler)
                                                                     .Select(FetchExistingTopicSubscriptions)
                                                                     .ToArray();

                                 Task.WaitAll(subscriptionTasks.Cast<Task>().ToArray());

                                 var subscriptionKeys = subscriptionTasks
                                     .SelectMany(t => t.Result)
                                     .OrderBy(k => k)
                                     .ToArray();

                                 return new ConcurrentSet<string>(subscriptionKeys);
                             },
                             "Fetching existing subscriptions");
        }

        private Task<string[]> FetchExistingTopicSubscriptions(string topicPath)
        {
            return Task.Run(() =>
                            {
                                return _retry.DoAsync(async () =>
                                                            {
                                                                var subscriptions = await _namespaceManager().GetSubscriptionsAsync(topicPath);

                                                                return subscriptions
                                                                    .Select(s => s.Name)
                                                                    .Select(subscriptionName => BuildSubscriptionKey(topicPath, subscriptionName))
                                                                    .ToArray();
                                                            },
                                                      "Fetching topic subscriptions for " + topicPath);
                            }).ConfigureAwaitFalse();
        }

        private ConcurrentSet<string> FetchExistingQueues()
        {
            return _retry.Do(() =>
                             {
                                 var queuesAsync = _namespaceManager().GetQueuesAsync();
                                 if (!queuesAsync.Wait(_defaultTimeout)) throw new TimeoutException("Fetching existing queues failed. Messaging endpoint did not respond in time.");

                                 var queues = queuesAsync.Result;
                                 var queuePaths = queues.Select(q => q.Path)
                                                        .OrderBy(p => p)
                                                        .ToArray();
                                 return new ConcurrentSet<string>(queuePaths);
                             },
                             "Fetching existing queues");
        }

        public Task<bool> QueueExists(string queuePath)
        {
            return Task.Run(() => _knownQueues.Value.Contains(queuePath)).ConfigureAwaitFalse();
        }

        public Task<bool> TopicExists(string topicPath)
        {
            return Task.Run(() => _knownTopics.Value.Contains(topicPath)).ConfigureAwaitFalse();
        }

        private void EnsureTopicExists(string topicPath)
        {
            if (_knownTopics.Value.Contains(topicPath)) return;
            lock (LockFor(topicPath))
            {
                if (_knownTopics.Value.Contains(topicPath)) return;

                var topicDescription = new TopicDescription(topicPath)
                                       {
                                           DefaultMessageTimeToLive = _defaultMessageTimeToLive,
                                           EnableBatchedOperations = true,
                                           RequiresDuplicateDetection = false,
                                           SupportOrdering = false,
                                           AutoDeleteOnIdle = _autoDeleteOnIdle
                                       };

                _retry.Do(() =>
                          {
                              // We don't check for topic existence here because that introduces a race condition with any other bus participant that's
                              // launching at the same time. If it doesn't exist, we'll create it. If it does, we'll just continue on with life and
                              // update its configuration in a minute anyway.  -andrewh 8/12/2013
                              try
                              {
                                  _namespaceManager().CreateTopic(topicDescription);
                              }
                              catch (MessagingEntityAlreadyExistsException)
                              {
                              }
                              catch (MessagingException exc)
                              {
                                  if (!exc.Message.Contains("SubCode=40901")) throw;

                                  // SubCode=40901. Another conflicting operation is in progress. Let's see if it's created the topic for us.
                                  if (!_namespaceManager().TopicExists(topicPath)) throw new BusException("Topic creation for '{0}' failed".FormatWith(topicPath));
                              }

                              _knownTopics.Value.Add(topicPath);
                          },
                          "Creating topic " + topicPath);
            }
        }

        private void EnsureSubscriptionExists(string topicPath, string subscriptionName)
        {
            var subscriptionKey = BuildSubscriptionKey(topicPath, subscriptionName);

            if (_knownSubscriptions.Value.Contains(subscriptionKey)) return;
            lock (LockFor(subscriptionKey))
            {
                if (_knownSubscriptions.Value.Contains(subscriptionKey)) return;

                EnsureTopicExists(topicPath);

                _retry.Do(() =>
                          {
                              var subscriptionDescription = new SubscriptionDescription(topicPath, subscriptionName)
                                                            {
                                                                MaxDeliveryCount = _maxDeliveryAttempts,
                                                                DefaultMessageTimeToLive = _defaultMessageTimeToLive,
                                                                EnableDeadLetteringOnMessageExpiration = _enableDeadLetteringOnMessageExpiration,
                                                                EnableBatchedOperations = true,
                                                                RequiresSession = false,
                                                                AutoDeleteOnIdle = _autoDeleteOnIdle
                                                            };

                              try
                              {
                                  _namespaceManager().CreateSubscription(subscriptionDescription);
                              }
                              catch (MessagingEntityAlreadyExistsException)
                              {
                              }
                              catch (MessagingException exc)
                              {
                                  if (!exc.Message.Contains("SubCode=40901")) throw;

                                  // SubCode=40901. Another conflicting operation is in progress. Let's see if it's created the subscription for us.
                                  if (!_namespaceManager().SubscriptionExists(topicPath, subscriptionName))
                                      throw new BusException("Subscription creation for '{0}/{1}' failed".FormatWith(topicPath, subscriptionName));
                              }

                              _knownSubscriptions.Value.Add(subscriptionKey);
                          },
                          "Creating subscription " + subscriptionName + " for topic " + topicPath);
            }
        }

        internal void EnsureQueueExists(string queuePath)
        {
            if (_knownQueues.Value.Contains(queuePath)) return;

            lock (LockFor(queuePath))
            {
                if (_knownQueues.Value.Contains(queuePath)) return;

                _retry.Do(() =>
                          {
                              var queueDescription = new QueueDescription(queuePath)
                                                     {
                                                         MaxDeliveryCount = _maxDeliveryAttempts,
                                                         DefaultMessageTimeToLive = _defaultMessageTimeToLive,
                                                         EnableDeadLetteringOnMessageExpiration = true,
                                                         EnableBatchedOperations = true,
                                                         RequiresDuplicateDetection = false,
                                                         RequiresSession = false,
                                                         SupportOrdering = false,
                                                         AutoDeleteOnIdle = _autoDeleteOnIdle
                                                     };

                              // We don't check for queue existence here because that introduces a race condition with any other bus participant that's
                              // launching at the same time. If it doesn't exist, we'll create it. If it does, we'll just continue on with life and
                              // update its configuration in a minute anyway.  -andrewh 8/12/2013
                              try
                              {
                                  _namespaceManager().CreateQueue(queueDescription);
                              }
                              catch (MessagingEntityAlreadyExistsException)
                              {
                                  _namespaceManager().UpdateQueue(queueDescription);
                              }
                              catch (MessagingException exc)
                              {
                                  if (!exc.Message.Contains("SubCode=40901")) throw;

                                  // SubCode=40901. Another conflicting operation is in progress. Let's see if it's created the queue for us.
                                  if (!_namespaceManager().QueueExists(queuePath))
                                      throw new BusException($"Queue creation for '{queuePath}' failed due to a conflicting operation and that queue does not already exist.", exc)
                                          .WithData("QueuePath", queuePath);
                              }

                              _knownQueues.Value.Add(queuePath);
                          },
                          "Creating queue " + queuePath);
            }
        }

        private bool WeHaveAHandler(string topicPath)
        {
            var paths = _typeProvider.AllTypesHandledViaTopics().Select(_pathFactory.TopicPathFor);
            return paths.Contains(topicPath);
        }

        private object LockFor(string path)
        {
            return _locks.GetOrAdd(path, p => new object());
        }

        private static string BuildSubscriptionKey(string topicPath, string subscriptionName)
        {
            return "{0}/{1}".FormatWith(topicPath, subscriptionName);
        }
    }
}