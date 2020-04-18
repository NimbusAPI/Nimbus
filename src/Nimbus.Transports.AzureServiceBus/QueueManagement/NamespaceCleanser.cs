using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus.Management;
using Nimbus.ConcurrentCollections;
using Nimbus.Configuration;
using Nimbus.Configuration.Settings;
using Nimbus.InfrastructureContracts;

namespace Nimbus.Transports.AzureServiceBus.QueueManagement
{
    public class NamespaceCleanser : INamespaceCleanser
    {
        private readonly ConnectionStringSetting _connectionString;
        private readonly GlobalPrefixSetting _globalPrefix;
        private readonly ILogger _logger;

        private readonly ThreadSafeLazy<ManagementClient> _namespaceManager;

        public NamespaceCleanser(ConnectionStringSetting connectionString, GlobalPrefixSetting globalPrefix, ILogger logger)
        {
            _connectionString = connectionString;
            _globalPrefix = globalPrefix;
            _logger = logger;

            _namespaceManager = new ThreadSafeLazy<ManagementClient>(() => new ManagementClient(connectionString));
        }

        /// <summary>
        ///     Danger! Danger, Will Robinson!
        /// </summary>
        public async Task RemoveAllExistingNamespaceElements()
        {
            var queueDeletionTasks = (await _namespaceManager.Value.GetQueuesAsync())
                                                      .Select(q => q.Path)
                                                      .Select(DeleteQueue)
                                                      .ToArray();

            var topicDeletionTasks = (await _namespaceManager.Value.GetTopicsAsync())
                                                      .Select(t => t.Path)
                                                      .Select(DeleteTopic)
                                                      .ToArray();

            var allDeletionTasks = new Task[0]
                .Union(queueDeletionTasks)
                .Union(topicDeletionTasks)
                .ToArray();

            await Task.WhenAll(allDeletionTasks);
        }

        private async Task DeleteTopic(string topicPath)
        {
            if (!topicPath.StartsWith(_globalPrefix.Value)) return;

            _logger.Debug("Deleting topic {0}", topicPath);
            await _namespaceManager.Value.DeleteTopicAsync(topicPath);
        }

        private async Task DeleteQueue(string queuePath)
        {
            if (!queuePath.StartsWith(_globalPrefix.Value)) return;

            _logger.Debug("Deleting queue {0}", queuePath);
            await _namespaceManager.Value.DeleteQueueAsync(queuePath);
        }
    }
}