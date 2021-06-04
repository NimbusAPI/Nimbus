namespace Nimbus.Transports.AzureServiceBus2.QueueManagement
{
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
    using Nimbus.Transports.AzureServiceBus2.ConnectionManagement;
    using Nimbus.Transports.AzureServiceBus2.Filtering;

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
            this._managementClient = managementClient;
            this._connectionManager = connectionManager;
            this._maxDeliveryAttempts = maxDeliveryAttempts;
            this._logger = logger;
            this._retry = retry;
            this._typeProvider = typeProvider;
            this._defaultMessageTimeToLive = defaultMessageTimeToLive;
            this._autoDeleteOnIdle = autoDeleteOnIdle;
            this._defaultTimeout = defaultTimeout;
            this._enableDeadLetteringOnMessageExpiration = enableDeadLetteringOnMessageExpiration;
            this._globalPrefix = globalPrefix;
            this._sqlFilterExpressionGenerator = sqlFilterExpressionGenerator;
            this._pathFactory = pathFactory;

            this._knownTopics = new ThreadSafeLazy<ConcurrentSet<string>>(this.FetchExistingTopics);
            this._knownSubscriptions = new ThreadSafeLazy<ConcurrentSet<string>>(this.FetchExistingSubscriptions);
            this._knownQueues = new ThreadSafeLazy<ConcurrentSet<string>>(this.FetchExistingQueues);
        }

        public Task<IMessageSender> CreateMessageSender(string queuePath)
        {
            return Task.Run( () =>
                                  {
                                      this.EnsureQueueExists(queuePath);
                                      var messageSender = this._connectionManager.CreateMessageSender(queuePath);
                                      return messageSender;
                                  }).ConfigureAwaitFalse();
        }

        public Task<IMessageReceiver> CreateMessageReceiver(string queuePath)
        {
            return Task.Run( () =>
                                  {
                                      this.EnsureQueueExists(queuePath);
                                      var receiver =  this._connectionManager.CreateMessageReceiver(queuePath, ReceiveMode.ReceiveAndDelete);
                                      return receiver;
                                  }).ConfigureAwaitFalse();
        }

        public Task<ITopicClient> CreateTopicSender(string topicPath)
        {
            return Task.Run(() =>
                            {
                                this.EnsureTopicExists(topicPath);

                                return this._retry.Do(() =>
                                                 {
                                                     var topicClient = this._connectionManager.CreateTopicClient(topicPath);
                                                     
                                                     return topicClient;
                                                 },
                                                 "Creating topic sender for " + topicPath);
                            }).ConfigureAwaitFalse();
        }

        public Task<ISubscriptionClient> CreateSubscriptionReceiver(string topicPath, string subscriptionName, IFilterCondition filterCondition)
        {
            const string ruleName = "$Default";
            return Task.Run(() =>
                            {
                                this.EnsureSubscriptionExists(topicPath, subscriptionName);

                                var myOwnSubscriptionFilterCondition = new OrCondition(new MatchCondition(MessagePropertyKeys.RedeliveryToSubscriptionName, subscriptionName),
                                                                                       new IsNullCondition(MessagePropertyKeys.RedeliveryToSubscriptionName));
                                var combinedCondition = new AndCondition(filterCondition, myOwnSubscriptionFilterCondition);
                                var filterSql = this._sqlFilterExpressionGenerator.GenerateFor(combinedCondition);
                                
                                return this._retry.Do(async () =>
                                                 {
                                                     
                                                     var subscriptionClient = this._connectionManager
                                                         .CreateSubscriptionClient(topicPath, subscriptionName, ReceiveMode.ReceiveAndDelete);
                                                     var rules = await subscriptionClient.GetRulesAsync();
                                                     

                                                     if (rules.Any(r => r.Name == ruleName))
                                                     {
                                                         await subscriptionClient.RemoveRuleAsync(ruleName);    
                                                     }
                                                     
                                                     await subscriptionClient.AddRuleAsync(ruleName, new SqlFilter(filterSql));

                                                     return subscriptionClient;
                                                 },
                                                 "Creating subscription receiver for topic " + topicPath + " and subscription " + subscriptionName + " with filter expression " +
                                                 filterCondition);
                            }).ConfigureAwaitFalse();
        }

        public Task MarkQueueAsNonExistent(string queuePath)
        {
            return Task.Run(() => this._knownQueues.Value.Remove(queuePath)).ConfigureAwaitFalse();
        }

        public Task MarkTopicAsNonExistent(string topicPath)
        {
            return Task.Run(() => this._knownTopics.Value.Remove(topicPath)).ConfigureAwaitFalse();
        }

        public Task MarkSubscriptionAsNonExistent(string topicPath, string subscriptionName)
        {
            return Task.Run(() =>
                            {
                                this._knownSubscriptions.Value
                                                   .Where(path => path.StartsWith(topicPath))
                                                   .Do(key => this._knownSubscriptions.Value.Remove(key))
                                                   .Done();
                            }).ConfigureAwaitFalse();
        }

        public Task<IMessageSender> CreateDeadQueueMessageSender()
        {
            return this.CreateMessageSender(this._pathFactory.DeadLetterOfficePath());
        }

        public Task<IMessageReceiver> CreateDeadQueueMessageReceiver()
        {
            return this.CreateMessageReceiver(this._pathFactory.DeadLetterOfficePath());
        }

        private ConcurrentSet<string> FetchExistingTopics()
        {
            return this._retry.Do(() =>
                             {
                                 var topicsAsync = this._managementClient().GetTopicsAsync();
                                 if (!topicsAsync.Wait(this._defaultTimeout)) throw new TimeoutException("Fetching existing topics failed. Messaging endpoint did not respond in time.");

                                 var topics = topicsAsync.Result;
                                 var topicPaths = new ConcurrentSet<string>(topics.Select(t => t.Path)
                                                                                  .Where(p => p.StartsWith(this._globalPrefix.Value))
                                                                                  .OrderBy(p => p)
                                                                                  .ToArray());

                                 this._logger.Debug("Found {topicCount} existing topics", topicPaths.Count());
                                 
                                 return topicPaths;
                             },
                             "Fetching existing topics");
        }

        private ConcurrentSet<string> FetchExistingSubscriptions()
        {
            return this._retry.Do(() =>
                             {
                                 var subscriptionTasks = this._knownTopics.Value
                                                                     .Where(this.WeHaveAHandler)
                                                                     .Select(this.FetchExistingTopicSubscriptions)
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
                                return this._retry.DoAsync(async () =>
                                                            {
                                                                var subscriptions = await this._managementClient().GetSubscriptionsAsync(topicPath);

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
            return this._retry.Do(() =>
                             {
                                 var queuesAsync = this._managementClient().GetQueuesAsync();
                                 if (!queuesAsync.Wait(this._defaultTimeout)) throw new TimeoutException("Fetching existing queues failed. Messaging endpoint did not respond in time.");

                                 var queues = queuesAsync.Result;
                                 var queuePaths = queues.Select(q => q.Path)
                                                        .Where(p => p.StartsWith(this._globalPrefix.Value))
                                                        .OrderBy(p => p)
                                                        .ToArray();
                                 return new ConcurrentSet<string>(queuePaths);
                             },
                             "Fetching existing queues");
        }

        public Task<bool> QueueExists(string queuePath)
        {
            return Task.Run(() => this._knownQueues.Value.Contains(queuePath)).ConfigureAwaitFalse();
        }

        public Task<bool> TopicExists(string topicPath)
        {
            return Task.Run(() => this._knownTopics.Value.Contains(topicPath)).ConfigureAwaitFalse();
        }

        private void EnsureTopicExists(string topicPath)
        {
            if (this._knownTopics.Value.Contains(topicPath)) return;
            lock (this.LockFor(topicPath))
            {
                if (this._knownTopics.Value.Contains(topicPath)) return;

                var topicDescription = new TopicDescription(topicPath)
                                       {
                                           DefaultMessageTimeToLive = this._defaultMessageTimeToLive,
                                           EnableBatchedOperations = true,
                                           RequiresDuplicateDetection = false,
                                           SupportOrdering = false,
                                           AutoDeleteOnIdle = this._autoDeleteOnIdle
                                       };

                this._retry.Do(async () =>
                          {
                              // We don't check for topic existence here because that introduces a race condition with any other bus participant that's
                              // launching at the same time. If it doesn't exist, we'll create it. If it does, we'll just continue on with life and
                              // update its configuration in a minute anyway.  -andrewh 8/12/2013
                              try
                              {
                                  // var exists = await _managementClient().TopicExistsAsync(topicDescription.Path);
                                  // if (!exists)
                                  // {
                                  this._logger.Debug("Creating topic {topic}", topicDescription.Path);
                                  await this._managementClient().CreateTopicAsync(topicDescription); 
                                  this._logger.Debug("Topic created {topic}", topicDescription.Path);
                                      //}

                              }
                              catch (MessagingEntityAlreadyExistsException)
                              {
                                  this._logger.Warn("Topic already exists. Continuing");
                              }
                              catch (Exception exc)
                              {
                                  if (!exc.Message.Contains("SubCode=40901")) throw;
                              
                                  // SubCode=40901. Another conflicting operation is in progress. Let's see if it's created the topic for us.
                                  var exists = await this._managementClient().TopicExistsAsync(topicPath);
                                  if (!exists) throw new BusException("Topic creation for '{0}' failed".FormatWith(topicPath));
                              }

                              this._knownTopics.Value.Add(topicPath);
                          },
                          "Creating topic " + topicPath);
            }
        }

        private void EnsureSubscriptionExists(string topicPath, string subscriptionName)
        {
            var subscriptionKey = BuildSubscriptionKey(topicPath, subscriptionName);

            if (this._knownSubscriptions.Value.Contains(subscriptionKey)) return;
            lock (this.LockFor(subscriptionKey))
            {
                if (this._knownSubscriptions.Value.Contains(subscriptionKey)) return;

                this.EnsureTopicExists(topicPath);

                this._retry.Do(async () =>
                          {
                              var subscriptionDescription = new SubscriptionDescription(topicPath, subscriptionName)
                                                            {
                                                                MaxDeliveryCount = this._maxDeliveryAttempts,
                                                                DefaultMessageTimeToLive = this._defaultMessageTimeToLive,
                                                                EnableDeadLetteringOnMessageExpiration = this._enableDeadLetteringOnMessageExpiration,
                                                                EnableBatchedOperations = true,
                                                                RequiresSession = false,
                                                                AutoDeleteOnIdle = this._autoDeleteOnIdle
                                                            };

                              try
                              {
                                  await this._managementClient().CreateSubscriptionAsync(subscriptionDescription);
                              }
                              catch (MessagingEntityAlreadyExistsException)
                              {
                                  
                              }
                              catch (Exception exc)
                              {
                                  if (!exc.Message.Contains("SubCode=40901")) throw;
                              
                                  // SubCode=40901. Another conflicting operation is in progress. Let's see if it's created the subscription for us.
                                  var exists = await this._managementClient().SubscriptionExistsAsync(topicPath, subscriptionName);
                                  if (!exists)
                                      throw new BusException("Subscription creation for '{0}/{1}' failed".FormatWith(topicPath, subscriptionName));
                              }

                              this._knownSubscriptions.Value.Add(subscriptionKey);
                          },
                          "Creating subscription " + subscriptionName + " for topic " + topicPath);
            }
        }

        internal void EnsureQueueExists(string queuePath)
        {
            if (this._knownQueues.Value.Contains(queuePath)) return;

            lock (this.LockFor(queuePath))
            {
                if (this._knownQueues.Value.Contains(queuePath)) return;

                this._retry.Do(async () =>
                          {
                              var queueDescription = new QueueDescription(queuePath)
                                                     {
                                                         MaxDeliveryCount = this._maxDeliveryAttempts,
                                                         DefaultMessageTimeToLive = this._defaultMessageTimeToLive,
                                                         EnableDeadLetteringOnMessageExpiration = true,
                                                         EnableBatchedOperations = true,
                                                         RequiresDuplicateDetection = false,
                                                         RequiresSession = false,
                                                         //SupportOrdering = false,
                                                         AutoDeleteOnIdle = this._autoDeleteOnIdle
                                                     };

                              // We don't check for queue existence here because that introduces a race condition with any other bus participant that's
                              // launching at the same time. If it doesn't exist, we'll create it. If it does, we'll just continue on with life and
                              // update its configuration in a minute anyway.  -andrewh 8/12/2013
                              try
                              {
                                  await this._managementClient().CreateQueueAsync(queueDescription);
                              }
                              catch (MessagingEntityAlreadyExistsException)
                              {
                                  await this._managementClient().UpdateQueueAsync(queueDescription);
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

                              this._knownQueues.Value.Add(queuePath);
                          },
                          "Creating queue " + queuePath);
            }
        }

        private bool WeHaveAHandler(string topicPath)
        {
            var paths = this._typeProvider.AllTypesHandledViaTopics().Select(this._pathFactory.TopicPathFor);
            return paths.Contains(topicPath);
        }

        private object LockFor(string path)
        {
            return this._locks.GetOrAdd(path, p => new object());
        }

        private static string BuildSubscriptionKey(string topicPath, string subscriptionName)
        {
            return "{0}/{1}".FormatWith(topicPath, subscriptionName);
        }
    }
}