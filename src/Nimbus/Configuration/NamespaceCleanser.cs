using System.Linq;
using System.Threading.Tasks;
using Microsoft.ServiceBus;
using Nimbus.ConcurrentCollections;
using Nimbus.Configuration.Settings;

namespace Nimbus.Configuration
{
    public class NamespaceCleanser : INamespaceCleanser
    {
        private readonly ConnectionStringSetting _connectionString;
        private readonly ILogger _logger;

        private readonly ThreadSafeLazy<NamespaceManager> _namespaceManager;

        public NamespaceCleanser(ConnectionStringSetting connectionString, ILogger logger)
        {
            _connectionString = connectionString;
            _logger = logger;

            _namespaceManager = new ThreadSafeLazy<NamespaceManager>(() => NamespaceManager.CreateFromConnectionString(_connectionString));
        }

        /// <summary>
        ///     Danger! Danger, Will Robinson!
        /// </summary>
        public async Task RemoveAllExistingNamespaceElements()
        {
            _logger.Debug("Removing all existing namespace elements. IMPORTANT: This should only be done in your regression test suites.");

            var queueDeletionTasks = _namespaceManager.Value.GetQueues()
                                                      .Select(q => q.Path)
                                                      .Select(queuePath => Task.Run(async delegate
                                                                                          {
                                                                                              _logger.Debug("Deleting queue {0}", queuePath);
                                                                                              await _namespaceManager.Value.DeleteQueueAsync(queuePath);
                                                                                          }))
                                                      .ToArray();

            var topicDeletionTasks = _namespaceManager.Value.GetTopics()
                                                      .Select(t => t.Path)
                                                      .Select(topicPath => Task.Run(async delegate
                                                                                          {
                                                                                              _logger.Debug("Deleting topic {0}", topicPath);
                                                                                              await _namespaceManager.Value.DeleteTopicAsync(topicPath);
                                                                                          }))
                                                      .ToArray();

            var allDeletionTasks = new Task[0]
                .Union(queueDeletionTasks)
                .Union(topicDeletionTasks)
                .ToArray();

            await Task.WhenAll(allDeletionTasks);
        }
    }
}