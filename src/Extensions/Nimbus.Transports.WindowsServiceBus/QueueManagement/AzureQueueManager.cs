using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Nimbus.ConcurrentCollections;
using Nimbus.Configuration.Settings;
using Nimbus.Extensions;
using Nimbus.Infrastructure;
using Nimbus.MessageContracts.Exceptions;

namespace Nimbus.Transports.WindowsServiceBus.QueueManagement
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
        private readonly ILogger _logger;

        private readonly ThreadSafeLazy<ConcurrentBag<string>> _knownTopics;
        private readonly ThreadSafeLazy<ConcurrentBag<string>> _knownSubscriptions;
        private readonly ThreadSafeLazy<ConcurrentBag<string>> _knownQueues;
        private readonly ITypeProvider _typeProvider;

        private readonly ThreadSafeDictionary<string, object> _locks = new ThreadSafeDictionary<string, object>();
        private readonly Retry _retry;

        public AzureQueueManager(Func<NamespaceManager> namespaceManager,
                                 Func<MessagingFactory> messagingFactory,
                                 MaxDeliveryAttemptSetting maxDeliveryAttempts,
                                 ILogger logger,
                                 ITypeProvider typeProvider,
                                 DefaultMessageTimeToLiveSetting defaultMessageTimeToLive,
                                 AutoDeleteOnIdleSetting autoDeleteOnIdle,
                                 DefaultTimeoutSetting defaultTimeout,
                                 EnableDeadLetteringOnMessageExpirationSetting enableDeadLetteringOnMessageExpiration)
        {
            _namespaceManager = namespaceManager;
            _messagingFactory = messagingFactory;
            _maxDeliveryAttempts = maxDeliveryAttempts;
            _logger = logger;
            _typeProvider = typeProvider;
            _defaultMessageTimeToLive = defaultMessageTimeToLive;
            _autoDeleteOnIdle = autoDeleteOnIdle;
            _defaultTimeout = defaultTimeout;
            _enableDeadLetteringOnMessageExpiration = enableDeadLetteringOnMessageExpiration;

            _knownTopics = new ThreadSafeLazy<ConcurrentBag<string>>(FetchExistingTopics);
            _knownSubscriptions = new ThreadSafeLazy<ConcurrentBag<string>>(FetchExistingSubscriptions);
            _knownQueues = new ThreadSafeLazy<ConcurrentBag<string>>(FetchExistingQueues);

            _retry = new Retry(5)
                .Chain(r => r.Started += (s, e) => _logger.Debug("{Action}...", e.ActionName))
                .Chain(r => r.Success += (s, e) => _logger.Debug("{Action} completed successfully in {Elapsed}.", e.ActionName, e.ElapsedTime))
                .Chain(r => r.TransientFailure += (s, e) => _logger.Warn(e.Exception, "A transient failure occurred in action {Action}.", e.ActionName))
                .Chain(r => r.PermanentFailure += (s, e) => _logger.Error(e.Exception, "A permanent failure occurred in action {Action}.", e.ActionName));
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

        public Task<SubscriptionClient> CreateSubscriptionReceiver(string topicPath, string subscriptionName)
        {
            return Task.Run(() =>
                            {
                                EnsureSubscriptionExists(topicPath, subscriptionName);

                                return _retry.Do(() =>
                                                 {
                                                     var subscriptionClient = _messagingFactory()
                                                         .CreateSubscriptionClient(topicPath, subscriptionName, ReceiveMode.ReceiveAndDelete);
                                                     return subscriptionClient;
                                                 },
                                                 "Creating subscription receiver for topic " + topicPath + " and subscription " + subscriptionName);
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

        private ConcurrentBag<string> FetchExistingTopics()
        {
            return _retry.Do(() =>
                             {
                                 var topicsAsync = _namespaceManager().GetTopicsAsync();
                                 if (!topicsAsync.Wait(_defaultTimeout)) throw new TimeoutException("Fetching existing topics failed. Messaging endpoint did not respond in time.");

                                 var topics = topicsAsync.Result;
                                 var topicPaths = new ConcurrentBag<string>(topics.Select(t => t.Path));

                                 return topicPaths;
                             },
                             "Fetching existing topics");
        }

        private ConcurrentBag<string> FetchExistingSubscriptions()
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

                                 return new ConcurrentBag<string>(subscriptionKeys);
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

        private ConcurrentBag<string> FetchExistingQueues()
        {
            return _retry.Do(() =>
                             {
                                 var queuesAsync = _namespaceManager().GetQueuesAsync();
                                 if (!queuesAsync.Wait(_defaultTimeout)) throw new TimeoutException("Fetching existing queues failed. Messaging endpoint did not respond in time.");

                                 var queues = queuesAsync.Result;
                                 var queuePaths = queues.Select(q => q.Path)
                                                        .OrderBy(p => p)
                                                        .ToArray();
                                 return new ConcurrentBag<string>(queuePaths);
                             },
                             "Fetching existing queues");
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
                                      throw new BusException("Queue creation for '{0}' failed".FormatWith(queuePath), exc);
                              }

                              _knownQueues.Value.Add(queuePath);
                          },
                          "Creating queue " + queuePath);
            }
        }

        private bool WeHaveAHandler(string topicPath)
        {
            var paths = _typeProvider.AllTypesHandledViaTopics().Select(PathFactory.TopicPathFor);
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