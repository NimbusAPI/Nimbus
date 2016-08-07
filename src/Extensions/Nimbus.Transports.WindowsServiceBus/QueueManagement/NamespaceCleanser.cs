using System.Linq;
using System.Threading.Tasks;
using Microsoft.ServiceBus;
using Nimbus.ConcurrentCollections;
using Nimbus.Configuration;
using Nimbus.Configuration.Settings;

namespace Nimbus.Transports.WindowsServiceBus.QueueManagement
{
    public class NamespaceCleanser : INamespaceCleanser
    {
        private readonly ConnectionStringSetting _connectionString;
        private readonly GlobalPrefixSetting _globalPrefix;
        private readonly ILogger _logger;

        private readonly ThreadSafeLazy<NamespaceManager> _namespaceManager;

        public NamespaceCleanser(ConnectionStringSetting connectionString, GlobalPrefixSetting globalPrefix, ILogger logger)
        {
            _connectionString = connectionString;
            _globalPrefix = globalPrefix;
            _logger = logger;

            _namespaceManager = new ThreadSafeLazy<NamespaceManager>(() => NamespaceManager.CreateFromConnectionString(_connectionString));
        }

        /// <summary>
        ///     Danger! Danger, Will Robinson!
        /// </summary>
        public async Task RemoveAllExistingNamespaceElements()
        {
            var queueDeletionTasks = _namespaceManager.Value.GetQueues()
                                                      .Select(q => q.Path)
                                                      .Select(DeleteQueue)
                                                      .ToArray();

            var topicDeletionTasks = _namespaceManager.Value.GetTopics()
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