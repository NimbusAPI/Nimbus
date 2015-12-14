using Nimbus.Infrastructure;
using Nimbus.Infrastructure.MessageSendersAndReceivers;

namespace Nimbus.Transports.InProcess
{
    internal class InProcessTransport : INimbusTransport
    {
        private readonly InProcessMessageStore _messageStore;

        public InProcessTransport(InProcessMessageStore messageStore)
        {
            _messageStore = messageStore;
        }

        public INimbusMessageSender GetQueueSender(string queuePath)
        {
            var queue = _messageStore.GetQueue(queuePath);
            return new InProcessQueueSender(queue);
        }

        public INimbusMessageReceiver GetQueueReceiver(string queuePath)
        {
            var queue = _messageStore.GetQueue(queuePath);
            return new InProcessQueueReceiver(queue);
        }

        public INimbusMessageSender GetTopicSender(string topicPath)
        {
            var topic = _messageStore.GetTopic(topicPath);
            return new InProcessTopicSender(_messageStore, topic);
        }

        public INimbusMessageReceiver GetTopicReceiver(string topicPath, string subscriptionName)
        {
            var subscriptionQueue = _messageStore.GetSubscriptionQueue(topicPath, subscriptionName);
            return new InProcessQueueReceiver(subscriptionQueue);
        }
    }
}