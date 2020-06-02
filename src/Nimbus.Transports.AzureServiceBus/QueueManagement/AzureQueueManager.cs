using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Azure.ServiceBus.Management;
using Nimbus.ConcurrentCollections;
using Nimbus.Configuration.Settings;
using Nimbus.Extensions;
using Nimbus.Infrastructure;
using Nimbus.Infrastructure.Retries;
using Nimbus.InfrastructureContracts;
using Nimbus.InfrastructureContracts.Filtering.Conditions;
using Nimbus.InfrastructureContracts.Routing;
using Nimbus.MessageContracts.Exceptions;
using Nimbus.Transports.AzureServiceBus.ConnectionManagement;
using Nimbus.Transports.AzureServiceBus.Filtering;

namespace Nimbus.Transports.AzureServiceBus.QueueManagement
{
    internal class AzureQueueManager : IQueueManager
    {
        private readonly Func<ManagementClient> _managementClient;
        private readonly IConnectionManager _connectionManager;
        private readonly AutoDeleteOnIdleSetting _autoDeleteOnIdle;
        private readonly DefaultMessageTimeToLiveSetting _defaultMessageTimeToLive;
        private readonly DefaultTimeoutSetting _defaultTimeout;
        private readonly EnableDeadLetteringOnMessageExpirationSetting _enableDeadLetteringOnMessageExpiration;
        private readonly GlobalPrefixSetting _globalPrefix;
        private readonly MaxDeliveryAttemptSetting _maxDeliveryAttempts;
        private readonly ILogger _logger;

        private readonly ThreadSafeLazy<ConcurrentSet<string>> _knownTopics;
        private readonly ThreadSafeLazy<ConcurrentSet<string>> _knownSubscriptions;
        private readonly ThreadSafeLazy<ConcurrentSet<string>> _knownQueues;
        private readonly IPathFactory _pathFactory;
        private readonly IRetry _retry;
        private readonly ISqlFilterExpressionGenerator _sqlFilterExpressionGenerator;
        private readonly ITypeProvider _typeProvider;

        private readonly ThreadSafeDictionary<string, object> _locks = new ThreadSafeDictionary<string, object>();

        public AzureQueueManager(Func<ManagementClient> managementClient,
                                 IConnectionManager connectionManager,
                                 AutoDeleteOnIdleSetting autoDeleteOnIdle,
                                 DefaultMessageTimeToLiveSetting defaultMessageTimeToLive,
                                 DefaultTimeoutSetting defaultTimeout,
                                 EnableDeadLetteringOnMessageExpirationSetting enableDeadLetteringOnMessageExpiration,
                                 GlobalPrefixSetting globalPrefix,
                                 MaxDeliveryAttemptSetting maxDeliveryAttempts,
                                 ILogger logger,
                                 IPathFactory pathFactory,
                                 IRetry retry,
                                 ISqlFilterExpressionGenerator sqlFilterExpressionGenerator,
                                 ITypeProvider typeProvider)
        {
            _managementClient = managementClient;
            _connectionManager = connectionManager;
            _maxDeliveryAttempts = maxDeliveryAttempts;
            _logger = logger;
            _retry = retry;
            _typeProvider = typeProvider;
            _defaultMessageTimeToLive = defaultMessageTimeToLive;
            _autoDeleteOnIdle = autoDeleteOnIdle;
            _defaultTimeout = defaultTimeout;
            _enableDeadLetteringOnMessageExpiration = enableDeadLetteringOnMessageExpiration;
            _globalPrefix = globalPrefix;
            _sqlFilterExpressionGenerator = sqlFilterExpressionGenerator;
            _pathFactory = pathFactory;

            _knownTopics = new ThreadSafeLazy<ConcurrentSet<string>>(FetchExistingTopics);
            _knownSubscriptions = new ThreadSafeLazy<ConcurrentSet<string>>(FetchExistingSubscriptions);
            _knownQueues = new ThreadSafeLazy<ConcurrentSet<string>>(FetchExistingQueues);
        }

        public Task<IMessageSender> CreateMessageSender(string queuePath)
        {
            return Task.Run( () =>
                                  {
                                      EnsureQueueExists(queuePath);
                                      var messageSender = _connectionManager.CreateMessageSender(queuePath);
                                      return messageSender;
                                  }).ConfigureAwaitFalse();
        }

        public Task<IMessageReceiver> CreateMessageReceiver(string queuePath)
        {
            return Task.Run( () =>
                                  {
                                      EnsureQueueExists(queuePath);
                                      var receiver =  _connectionManager.CreateMessageReceiver(queuePath, ReceiveMode.ReceiveAndDelete);
                                      return receiver;
                                  }).ConfigureAwaitFalse();
        }

        public Task<ITopicClient> CreateTopicSender(string topicPath)
        {
            return Task.Run(() =>
                            {
                                EnsureTopicExists(topicPath);

                                return _retry.Do(() =>
                                                 {
                                                     var topicClient = _connectionManager.CreateTopicClient(topicPath);
                                                     
                                                     return topicClient;
                                                 },
                                                 "Creating topic sender for " + topicPath);
                            }).ConfigureAwaitFalse();
        }

        public Task<ISubscriptionClient> CreateSubscriptionReceiver(string topicPath, string subscriptionName, IFilterCondition filterCondition)
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
                                                     var subscriptionClient = _connectionManager
                                                         .CreateSubscriptionClient(topicPath, subscriptionName, ReceiveMode.ReceiveAndDelete);
                                                     
                                                     subscriptionClient.AddRuleAsync("$Default", new SqlFilter(filterSql));
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

        public Task<IMessageSender> CreateDeadQueueMessageSender()
        {
            return CreateMessageSender(_pathFactory.DeadLetterOfficePath());
        }

        public Task<IMessageReceiver> CreateDeadQueueMessageReceiver()
        {
            return CreateMessageReceiver(_pathFactory.DeadLetterOfficePath());
        }

        private ConcurrentSet<string> FetchExistingTopics()
        {
            return _retry.Do(() =>
                             {
                                 var topicsAsync = _managementClient().GetTopicsAsync();
                                 if (!topicsAsync.Wait(_defaultTimeout)) throw new TimeoutException("Fetching existing topics failed. Messaging endpoint did not respond in time.");

                                 var topics = topicsAsync.Result;
                                 var topicPaths = new ConcurrentSet<string>(topics.Select(t => t.Path)
                                                                                  .Where(p => p.StartsWith(_globalPrefix.Value))
                                                                                  .OrderBy(p => p)
                                                                                  .ToArray());

                                 _logger.Debug("Found {topicCount} existing topics", topicPaths.Count());
                                 
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
                                                                var subscriptions = await _managementClient().GetSubscriptionsAsync(topicPath);

                                                                return subscriptions
                                                                    .Select(s => s.SubscriptionName)
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
                                 var queuesAsync = _managementClient().GetQueuesAsync();
                                 if (!queuesAsync.Wait(_defaultTimeout)) throw new TimeoutException("Fetching existing queues failed. Messaging endpoint did not respond in time.");

                                 var queues = queuesAsync.Result;
                                 var queuePaths = queues.Select(q => q.Path)
                                                        .Where(p => p.StartsWith(_globalPrefix.Value))
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

                _retry.Do(async () =>
                          {
                              // We don't check for topic existence here because that introduces a race condition with any other bus participant that's
                              // launching at the same time. If it doesn't exist, we'll create it. If it does, we'll just continue on with life and
                              // update its configuration in a minute anyway.  -andrewh 8/12/2013
                              try
                              {
                                  await _managementClient().CreateTopicAsync(topicDescription);
                              }
                              catch (MessagingEntityAlreadyExistsException)
                              {
                              }
                              catch (Exception exc)
                              {
                                  if (!exc.Message.Contains("SubCode=40901")) throw;
                              
                                  // SubCode=40901. Another conflicting operation is in progress. Let's see if it's created the topic for us.
                                  var exists = await _managementClient().TopicExistsAsync(topicPath);
                                  if (!exists) throw new BusException("Topic creation for '{0}' failed".FormatWith(topicPath));
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

                _retry.Do(async () =>
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
                                  await _managementClient().CreateSubscriptionAsync(subscriptionDescription);
                              }
                              catch (MessagingEntityAlreadyExistsException)
                              {
                                  
                              }
                              catch (Exception exc)
                              {
                                  if (!exc.Message.Contains("SubCode=40901")) throw;
                              
                                  // SubCode=40901. Another conflicting operation is in progress. Let's see if it's created the subscription for us.
                                  var exists = await _managementClient().SubscriptionExistsAsync(topicPath, subscriptionName);
                                  if (!exists)
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

                _retry.Do(async () =>
                          {
                              var queueDescription = new QueueDescription(queuePath)
                                                     {
                                                         MaxDeliveryCount = _maxDeliveryAttempts,
                                                         DefaultMessageTimeToLive = _defaultMessageTimeToLive,
                                                         EnableDeadLetteringOnMessageExpiration = true,
                                                         EnableBatchedOperations = true,
                                                         RequiresDuplicateDetection = false,
                                                         RequiresSession = false,
                                                         //SupportOrdering = false,
                                                         AutoDeleteOnIdle = _autoDeleteOnIdle
                                                     };

                              // We don't check for queue existence here because that introduces a race condition with any other bus participant that's
                              // launching at the same time. If it doesn't exist, we'll create it. If it does, we'll just continue on with life and
                              // update its configuration in a minute anyway.  -andrewh 8/12/2013
                              try
                              {
                                  await _managementClient().CreateQueueAsync(queueDescription);
                              }
                              catch (MessagingEntityAlreadyExistsException)
                              {
                                  await _managementClient().UpdateQueueAsync(queueDescription);
                              }
                              // catch (MessagingException exc)
                              // {
                              //     if (!exc.Message.Contains("SubCode=40901")) throw;
                              //
                              //     // SubCode=40901. Another conflicting operation is in progress. Let's see if it's created the queue for us.
                              //     if (!_managementClient().QueueExists(queuePath))
                              //         throw new BusException($"Queue creation for '{queuePath}' failed due to a conflicting operation and that queue does not already exist.", exc)
                              //             .WithData("QueuePath", queuePath);
                              // }

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