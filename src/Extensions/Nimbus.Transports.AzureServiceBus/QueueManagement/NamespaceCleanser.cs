using System.Linq;
using System.Threading.Tasks;
using Microsoft.ServiceBus;
using Nimbus.ConcurrentCollections;
using Nimbus.Configuration;
using Nimbus.Configuration.Settings;
using Nimbus.Transports.AzureServiceBus.Configuration;

namespace Nimbus.Transports.AzureServiceBus.QueueManagement
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
            var namespaceManager = _namespaceManager.Value;
            var queuesTask = namespaceManager.GetQueuesAsync();
            var topicsTask = namespaceManager.GetTopicsAsync();

            var queues = await queuesTask;
            var queueDeletionTasks = queues
                .Select(q => q.Path)
                .Where(BelongsToMe)
                .Select(DeleteQueue)
                .ToArray();

            var topics = await topicsTask;
            var topicDeletionTasks = topics
                .Select(t => t.Path)
                .Where(BelongsToMe)
                .Select(DeleteTopic)
                .ToArray();

            var allDeletionTasks = new Task[0]
                .Union(queueDeletionTasks)
                .Union(topicDeletionTasks)
                .ToArray();

            _logger.Debug("Deleting {QueueCount} queues and {TopicCount} topics (total {TotalCount})",
                          queueDeletionTasks.Length,
                          topicDeletionTasks.Length,
                          queueDeletionTasks.Length + topicDeletionTasks.Length);

            await Task.WhenAll(allDeletionTasks);
        }

        private bool BelongsToMe(string queueOrTopicPath)
        {
            var belongsToMe = queueOrTopicPath.StartsWith(_globalPrefix.Value);
            return belongsToMe;
        }

        private async Task DeleteTopic(string topicPath)
        {
            _logger.Debug("Deleting topic {0}", topicPath);
            await _namespaceManager.Value.DeleteTopicAsync(topicPath);
        }

        private async Task DeleteQueue(string queuePath)
        {
            _logger.Debug("Deleting queue {0}", queuePath);
            await _namespaceManager.Value.DeleteQueueAsync(queuePath);
        }
    }
}