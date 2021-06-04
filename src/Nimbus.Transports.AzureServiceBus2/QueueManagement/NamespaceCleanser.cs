namespace Nimbus.Transports.AzureServiceBus2.QueueManagement
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Azure.Messaging.ServiceBus.Administration;
    using Nimbus.ConcurrentCollections;
    using Nimbus.Configuration;
    using Nimbus.Configuration.Settings;
    using Nimbus.InfrastructureContracts;

    public class NamespaceCleanser : INamespaceCleanser
    {
        private readonly ConnectionStringSetting _connectionString;
        private readonly GlobalPrefixSetting _globalPrefix;
        private readonly ILogger _logger;

        private readonly ThreadSafeLazy<ServiceBusAdministrationClient> _namespaceManager;

        public NamespaceCleanser(ConnectionStringSetting connectionString, GlobalPrefixSetting globalPrefix, ILogger logger)
        {
            this._connectionString = connectionString;
            this._globalPrefix = globalPrefix;
            this._logger = logger;

            this._namespaceManager = new ThreadSafeLazy<ServiceBusAdministrationClient>(() => new ServiceBusAdministrationClient(connectionString));
        }

        /// <summary>
        ///     Danger! Danger, Will Robinson!
        /// </summary>
        public async Task RemoveAllExistingNamespaceElements()
        {
            var queuesAsync = this._namespaceManager.Value.GetQueuesAsync();
            var queuePages = queuesAsync.AsPages();
            var queues = new List<QueueProperties>();
            await foreach (var queuePage in queuePages)
            {
                foreach (var queue in queuePage.Values)
                {
                    queues.Add(queue);
                }
            }

            var queueDeletionTasks = queues
                                     .Select(q => q.Name)
                                     .Select(this.DeleteQueue)
                                     .ToArray();

            var topicsAsync = this._namespaceManager.Value.GetTopicsAsync();
            var topicPages = topicsAsync.AsPages();
            var topics = new List<TopicProperties>();
            await foreach (var topicPage in topicPages)
            {
                foreach (var topic in topicPage.Values)
                {
                    topics.Add(topic);
                }
            }

            var topicDeletionTasks = topics
                                     .Select(q => q.Name)
                                     .Select(this.DeleteTopic)
                                     .ToArray();

            var allDeletionTasks = new Task[0]
                                   .Union(queueDeletionTasks)
                                   .Union(topicDeletionTasks)
                                   .ToArray();

            await Task.WhenAll(allDeletionTasks);
        }

        private async Task DeleteTopic(string topicPath)
        {
            if (!topicPath.StartsWith(this._globalPrefix.Value)) return;
            
            var subscriptionsAsync = this._namespaceManager.Value.GetSubscriptionsAsync(topicPath);
            var subscriptionPages = subscriptionsAsync.AsPages();
            var subscriptions = new List<SubscriptionProperties>();
            await foreach (var subscriptionPage in subscriptionPages)
            {
                foreach (var subscription in subscriptionPage.Values)
                {
                    subscriptions.Add(subscription);
                }
            }
            
            

            var subscriptionDeletionTasks = subscriptions
                                            .Select(s => this.DeleteSubscription(topicPath, s.SubscriptionName))
                                            .ToArray();

            this._logger.Debug($"Deleting {subscriptions.Count} subscriptions for {topicPath}");
            await Task.WhenAll(subscriptionDeletionTasks);

            this._logger.Debug("Deleting topic {0}", topicPath);
            await this._namespaceManager.Value.DeleteTopicAsync(topicPath);
            this._logger.Debug("Deleted topic {0}", topicPath);
        }

        private async Task DeleteSubscription(string topicPath, string subscriptionName)
        {
            await this._namespaceManager.Value.DeleteSubscriptionAsync(topicPath, subscriptionName);
        }

        private async Task DeleteQueue(string queuePath)
        {
            if (!queuePath.StartsWith(this._globalPrefix.Value)) return;

            this._logger.Debug("Deleting queue {0}", queuePath);
            await this._namespaceManager.Value.DeleteQueueAsync(queuePath);
            this._logger.Debug("Deleted queue {0}", queuePath);
        }
    }
}