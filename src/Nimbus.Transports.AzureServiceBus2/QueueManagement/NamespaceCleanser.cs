namespace Nimbus.Transports.AzureServiceBus2.QueueManagement
{
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Azure.ServiceBus.Management;
    using Nimbus.ConcurrentCollections;
    using Nimbus.Configuration;
    using Nimbus.Configuration.Settings;
    using Nimbus.InfrastructureContracts;

    public class NamespaceCleanser : INamespaceCleanser
    {
        private readonly ConnectionStringSetting _connectionString;
        private readonly GlobalPrefixSetting _globalPrefix;
        private readonly ILogger _logger;

        private readonly ThreadSafeLazy<ManagementClient> _namespaceManager;

        public NamespaceCleanser(ConnectionStringSetting connectionString, GlobalPrefixSetting globalPrefix, ILogger logger)
        {
            this._connectionString = connectionString;
            this._globalPrefix = globalPrefix;
            this._logger = logger;

            this._namespaceManager = new ThreadSafeLazy<ManagementClient>(() => new ManagementClient(connectionString));
        }

        /// <summary>
        ///     Danger! Danger, Will Robinson!
        /// </summary>
        public async Task RemoveAllExistingNamespaceElements()
        {
            var queueDeletionTasks = (await this._namespaceManager.Value.GetQueuesAsync())
                                                      .Select(q => q.Path)
                                                      .Select(this.DeleteQueue)
                                                      .ToArray();
            
            var topicDeletionTasks = (await this._namespaceManager.Value.GetTopicsAsync())
                                                      .Select(t => t.Path)
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

            var subscriptions = await this._namespaceManager.Value.GetSubscriptionsAsync(topicPath);
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