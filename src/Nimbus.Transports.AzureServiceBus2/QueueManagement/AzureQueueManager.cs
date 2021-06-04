namespace Nimbus.Transports.AzureServiceBus2.QueueManagement
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Azure.Messaging.ServiceBus;
    using Azure.Messaging.ServiceBus.Administration;
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
        private readonly Func<ServiceBusAdministrationClient> _administrationClient;
        private readonly IConnectionManager _connectionManager;
        private readonly AutoDeleteOnIdleSetting _autoDeleteOnIdle;
        private readonly DefaultMessageTimeToLiveSetting _defaultMessageTimeToLive;
        private readonly DefaultTimeoutSetting _defaultTimeout;
        private readonly EnableDeadLetteringOnMessageExpirationSetting _enableDeadLetteringOnMessageExpiration;
        private readonly GlobalPrefixSetting _globalPrefix;
        private readonly MaxDeliveryAttemptSetting _maxDeliveryAttempts;
        private readonly ILogger _logger;

        private readonly ThreadSafeLazy<Task<ConcurrentSet<string>>> _knownTopics;
        private readonly ThreadSafeLazy<Task<ConcurrentSet<string>>> _knownSubscriptions;
        private readonly ThreadSafeLazy<Task<ConcurrentSet<string>>> _knownQueues;
        private readonly IPathFactory _pathFactory;
        private readonly IRetry _retry;
        private readonly ISqlFilterExpressionGenerator _sqlFilterExpressionGenerator;
        private readonly ITypeProvider _typeProvider;

        private readonly ThreadSafeDictionary<string, object> _locks = new ThreadSafeDictionary<string, object>();

        public AzureQueueManager(
            Func<ServiceBusAdministrationClient> administrationClient,
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
            this._administrationClient = administrationClient;
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

            this._knownTopics = new ThreadSafeLazy<Task<ConcurrentSet<string>>>(this.FetchExistingTopics);
            this._knownSubscriptions = new ThreadSafeLazy<Task<ConcurrentSet<string>>>(this.FetchExistingSubscriptions);
            this._knownQueues = new ThreadSafeLazy<Task<ConcurrentSet<string>>>(this.FetchExistingQueues);
        }

        public Task<ServiceBusSender> CreateMessageSender(string queuePath)
        {
            return Task.Run(
                () =>
                {
                    this.EnsureQueueExists(queuePath);
                    var messageSender = this._connectionManager.CreateMessageSender(queuePath);
                    return messageSender;
                }).ConfigureAwaitFalse();
        }

        public Task<ServiceBusReceiver> CreateMessageReceiver(string queuePath, int preFetchCount = 0)
        {
            return Task.Run(
                () =>
                {
                    this.EnsureQueueExists(queuePath);
                    var receiver = this._connectionManager.CreateMessageReceiver(queuePath, ServiceBusReceiveMode.ReceiveAndDelete, preFetchCount);
                    return receiver;
                }).ConfigureAwaitFalse();
        }

        public Task<ServiceBusSender> CreateTopicSender(string topicPath)
        {
            return Task.Run(
                () =>
                {
                    this.EnsureTopicExists(topicPath);

                    return this._retry.Do(
                        () =>
                        {
                            var topicClient = this._connectionManager.CreateTopicClient(topicPath);

                            return topicClient;
                        },
                        "Creating topic sender for " + topicPath);
                }).ConfigureAwaitFalse();
        }

        public Task<ServiceBusProcessor> CreateSubscriptionReceiver(
            string topicPath,
            string subscriptionName,
            IFilterCondition filterCondition,
            int preFetchCount)
        {
            const string ruleName = "$Default";
            return Task.Run(
                () =>
                {
                    this.EnsureSubscriptionExists(topicPath, subscriptionName);

                    var myOwnSubscriptionFilterCondition = new OrCondition(
                        new MatchCondition(MessagePropertyKeys.RedeliveryToSubscriptionName, subscriptionName),
                        new IsNullCondition(MessagePropertyKeys.RedeliveryToSubscriptionName));
                    var combinedCondition = new AndCondition(filterCondition, myOwnSubscriptionFilterCondition);
                    var filterSql = this._sqlFilterExpressionGenerator.GenerateFor(combinedCondition);

                    return this._retry.Do(
                        async () =>
                        {
                            var rulesAsync = this._administrationClient().GetRulesAsync(topicPath, subscriptionName);

                            var pages = rulesAsync.AsPages();
                            var rules = new List<RuleProperties>();
                            await foreach (var rulePage in pages)
                            {
                                foreach (var rule in rulePage.Values)
                                {
                                    if (rule.Name.StartsWith(this._globalPrefix.Value))
                                    {
                                        rules.Add(rule);
                                    }
                                }
                            }

                            if (rules.Any(r => r.Name == ruleName))
                            {
                                await this._administrationClient().DeleteRuleAsync(topicPath, subscriptionName, ruleName);
                            }

                            var options = new CreateRuleOptions()
                                          {
                                              Filter = new SqlRuleFilter(filterSql),
                                              Name = ruleName
                                          };
                            await this._administrationClient().CreateRuleAsync(topicPath, subscriptionName, options);

                            return this._connectionManager
                                       .CreateSubscriptionClient(topicPath, subscriptionName, ServiceBusReceiveMode.ReceiveAndDelete, preFetchCount);
                        },
                        "Creating subscription receiver for topic " + topicPath + " and subscription " + subscriptionName + " with filter expression " +
                        filterCondition);
                }).ConfigureAwaitFalse();
        }

        public async Task MarkQueueAsNonExistent(string queuePath)
        {
            var value = await this._knownQueues.Value;
            value.Remove(queuePath);
        }

        public async Task MarkTopicAsNonExistent(string topicPath)
        {
            var value = await this._knownTopics.Value;
            value.Remove(topicPath);
        }

        public async Task MarkSubscriptionAsNonExistent(string topicPath, string subscriptionName)
        {
            var value = await this._knownSubscriptions.Value;
            value
                .Where(path => path.StartsWith(topicPath))
                .Do(key => value.Remove(key))
                .Done();
        }

        public Task<ServiceBusSender> CreateDeadQueueMessageSender()
        {
            return this.CreateMessageSender(this._pathFactory.DeadLetterOfficePath());
        }

        public Task<ServiceBusReceiver> CreateDeadQueueMessageReceiver()
        {
            return this.CreateMessageReceiver(this._pathFactory.DeadLetterOfficePath());
        }

        private Task<ConcurrentSet<string>> FetchExistingTopics()
        {
            return Task.Run(
                () =>
                {
                    return this._retry.DoAsync(
                        async () =>
                        {
                            var topicsAsync = this._administrationClient().GetTopicsAsync();

                            var pages = topicsAsync.AsPages();
                            var topicPaths = new List<string>();
                            await foreach (Azure.Page<TopicProperties> topicPage in pages)
                            {
                                foreach (var topicProperties in topicPage.Values)
                                {
                                    if (topicProperties.Name.StartsWith(this._globalPrefix.Value))
                                    {
                                        topicPaths.Add(topicProperties.Name);
                                    }
                                }
                            }

                            this._logger.Debug("Found {topicCount} existing topics", new ConcurrentSet<string>(topicPaths.OrderBy(p => p).ToArray()).Count());

                            return new ConcurrentSet<string>(topicPaths.OrderBy(p => p).ToArray());
                        },
                        "Fetching existing topics");
                }).ConfigureAwaitFalse();
        }

        private Task<ConcurrentSet<string>> FetchExistingSubscriptions()
        {
            return this._retry.DoAsync(
                async () =>
                {
                    var value = await this._knownTopics.Value;
                    var subscriptionTasks = value.Where(this.WeHaveAHandler)
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
            return Task.Run(
                () =>
                {
                    return this._retry.DoAsync(
                        async () =>
                        {
                            var subscriptionsAsync = this._administrationClient().GetSubscriptionsAsync(topicPath);

                            var subscriptionPages = subscriptionsAsync.AsPages();
                            var subscriptions = new List<string>();
                            await foreach (var subscriptionPage in subscriptionPages)
                            {
                                foreach (var subscription in subscriptionPage.Values)
                                {
                                    subscriptions.Add(subscription.SubscriptionName);
                                }
                            }

                            return subscriptions
                                   .Select(subscriptionName => BuildSubscriptionKey(topicPath, subscriptionName))
                                   .ToArray();
                        },
                        "Fetching topic subscriptions for " + topicPath);
                }).ConfigureAwaitFalse();
        }

        private Task<ConcurrentSet<string>> FetchExistingQueues()
        {
            return this._retry.DoAsync(
                async () =>
                {
                    var queuesAsync = this._administrationClient().GetQueuesAsync();

                    var queuePages = queuesAsync.AsPages();
                    var queues = new List<string>();
                    await foreach (var queuePage in queuePages)
                    {
                        foreach (var queue in queuePage.Values)
                        {
                            queues.Add(queue.Name);
                        }
                    }

                    var queuePaths = queues.Where(p => p.StartsWith(this._globalPrefix.Value))
                                           .OrderBy(p => p)
                                           .ToArray();
                    return new ConcurrentSet<string>(queuePaths);
                },
                "Fetching existing queues");
        }

        public async Task<bool> QueueExists(string queuePath)
        {
            var value = await this._knownQueues.Value;
            return value.Contains(queuePath);
        }

        public async Task<bool> TopicExists(string topicPath)
        {
            var value = await this._knownTopics.Value;
            return value.Contains(topicPath);
        }

        private async Task EnsureTopicExists(string topicPath)
        {
            var value = await this._knownTopics.Value;
            if (value.Contains(topicPath)) return;
            lock (this.LockFor(topicPath))
            {
                if (value.Contains(topicPath)) return;

                var topicDescription = new CreateTopicOptions(topicPath)
                                       {
                                           DefaultMessageTimeToLive = this._defaultMessageTimeToLive,
                                           EnableBatchedOperations = true,
                                           RequiresDuplicateDetection = false,
                                           SupportOrdering = false,
                                           AutoDeleteOnIdle = this._autoDeleteOnIdle
                                       };

                this._retry.DoAsync(
                    async () =>
                    {
                        // We don't check for topic existence here because that introduces a race condition with any other bus participant that's
                        // launching at the same time. If it doesn't exist, we'll create it. If it does, we'll just continue on with life and
                        // update its configuration in a minute anyway.  -andrewh 8/12/2013
                        try
                        {
                            // var exists = await _managementClient().TopicExistsAsync(topicDescription.Path);
                            // if (!exists)
                            // {
                            this._logger.Debug("Creating topic {topic}", topicDescription.Name);
                            await this._administrationClient().CreateTopicAsync(topicDescription);
                            this._logger.Debug("Topic created {topic}", topicDescription.Name);
                            //}
                        }
                        catch (ServiceBusException ex) when (ex.Reason == ServiceBusFailureReason.MessagingEntityAlreadyExists)
                        {
                            this._logger.Warn("Topic already exists. Continuing");
                        }
                        catch (Exception exc)
                        {
                            if (!exc.Message.Contains("SubCode=40901")) throw;

                            // SubCode=40901. Another conflicting operation is in progress. Let's see if it's created the topic for us.
                            var exists = await this._administrationClient().TopicExistsAsync(topicPath);
                            if (!exists) throw new BusException("Topic creation for '{0}' failed".FormatWith(topicPath));
                        }

                        value.Add(topicPath);
                    },
                    "Creating topic " + topicPath);
            }
        }

        private async Task EnsureSubscriptionExists(string topicPath, string subscriptionName)
        {
            var subscriptionKey = BuildSubscriptionKey(topicPath, subscriptionName);

            var value = await this._knownSubscriptions.Value;
            if (value.Contains(subscriptionKey)) return;
            lock (this.LockFor(subscriptionKey))
            {
                if (value.Contains(subscriptionKey)) return;

                this.EnsureTopicExists(topicPath);

                this._retry.DoAsync(
                    async () =>
                    {
                        var subscriptionDescription = new CreateSubscriptionOptions(topicPath, subscriptionName)
                                                      {
                                                          MaxDeliveryCount = this._maxDeliveryAttempts,
                                                          DefaultMessageTimeToLive = this._defaultMessageTimeToLive,
                                                          DeadLetteringOnMessageExpiration = this._enableDeadLetteringOnMessageExpiration,
                                                          EnableBatchedOperations = true,
                                                          RequiresSession = false,
                                                          AutoDeleteOnIdle = this._autoDeleteOnIdle
                                                      };

                        try
                        {
                            await this._administrationClient().CreateSubscriptionAsync(subscriptionDescription);
                        }
                        catch (ServiceBusException ex) when (ex.Reason == ServiceBusFailureReason.MessagingEntityAlreadyExists)
                        {
                        }
                        catch (Exception exc)
                        {
                            if (!exc.Message.Contains("SubCode=40901")) throw;

                            // SubCode=40901. Another conflicting operation is in progress. Let's see if it's created the subscription for us.
                            var exists = await this._administrationClient().SubscriptionExistsAsync(topicPath, subscriptionName);
                            if (!exists)
                                throw new BusException("Subscription creation for '{0}/{1}' failed".FormatWith(topicPath, subscriptionName));
                        }

                        value.Add(subscriptionKey);
                    },
                    "Creating subscription " + subscriptionName + " for topic " + topicPath);
            }
        }

        internal async Task EnsureQueueExists(string queuePath)
        {
            var value = await this._knownQueues.Value;
            if (value.Contains(queuePath)) return;

            lock (this.LockFor(queuePath))
            {
                if (value.Contains(queuePath)) return;

                this._retry.DoAsync(
                    async () =>
                    {
                        var queueDescription = new CreateQueueOptions(queuePath)
                                               {
                                                   MaxDeliveryCount = this._maxDeliveryAttempts,
                                                   DefaultMessageTimeToLive = this._defaultMessageTimeToLive,
                                                   DeadLetteringOnMessageExpiration = true,
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
                            await this._administrationClient().CreateQueueAsync(queueDescription);
                        }
                        catch (ServiceBusException ex) when (ex.Reason == ServiceBusFailureReason.MessagingEntityAlreadyExists)
                        {
                            // Patrick TODO: How to update this?!?!?!
                            // await this._administrationClient().UpdateQueueAsync(queueDescription);
                        }

                        value.Add(queuePath);
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