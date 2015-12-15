using Nimbus.Configuration.PoorMansIocContainer;
using Nimbus.Infrastructure;
using Nimbus.Infrastructure.MessageSendersAndReceivers;
using Nimbus.Transports.InProcess.MessageSendersAndReceivers;

namespace Nimbus.Transports.InProcess
{
    internal class InProcessTransport : INimbusTransport
    {
        private readonly InProcessMessageStore _messageStore;
        private readonly PoorMansIoC _container;

        public InProcessTransport(InProcessMessageStore messageStore, PoorMansIoC container)
        {
            _messageStore = messageStore;
            _container = container;
        }

        public INimbusMessageSender GetQueueSender(string queuePath)
        {
            var queue = _messageStore.GetQueue(queuePath);
            return _container.ResolveWithOverrides<InProcessQueueSender>(queue);
        }

        public INimbusMessageReceiver GetQueueReceiver(string queuePath)
        {
            var queue = _messageStore.GetQueue(queuePath);
            return _container.ResolveWithOverrides<InProcessQueueReceiver>(queue);
        }

        public INimbusMessageSender GetTopicSender(string topicPath)
        {
            var topic = _messageStore.GetTopic(topicPath);
            return _container.ResolveWithOverrides<InProcessTopicSender>(topic);
        }

        public INimbusMessageReceiver GetTopicReceiver(string topicPath, string subscriptionName)
        {
            var topic = _messageStore.GetTopic(topicPath);
            var subscriptionQueue = topic.GetSubscriptionQueue(subscriptionName);
            return _container.ResolveWithOverrides<InProcessQueueReceiver>(subscriptionQueue);
        }
    }
}