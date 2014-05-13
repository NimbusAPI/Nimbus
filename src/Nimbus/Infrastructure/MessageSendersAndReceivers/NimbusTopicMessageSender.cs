using Microsoft.ServiceBus.Messaging;
using Nimbus.ConcurrentCollections;

namespace Nimbus.Infrastructure.MessageSendersAndReceivers
{
    internal class NimbusTopicMessageSender : BatchingMessageSender
    {
        private readonly IQueueManager _queueManager;
        private readonly string _topicPath;
        private readonly ILogger _logger;

        private readonly ThreadSafeLazy<TopicClient> _topicClient;

        public NimbusTopicMessageSender(IQueueManager queueManager, string topicPath, ILogger logger)
        {
            _queueManager = queueManager;
            _topicPath = topicPath;
            _logger = logger;

            _topicClient = new ThreadSafeLazy<TopicClient>(() => _queueManager.CreateTopicSender(_topicPath).Result);
        }

        protected override void SendBatch(BrokeredMessage[] toSend)
        {
            _logger.Debug("Flushing outbound message queue {0} ({1} messages)", _topicPath, toSend.Length);
            _topicClient.Value.SendBatch(toSend);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (!_topicClient.IsValueCreated) return;
            _topicClient.Value.Close();
        }
    }
}