using System.Threading.Tasks;
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

        public Task TestConnection()
        {
            return Task.Delay(0);
        }

        public INimbusMessageSender GetQueueSender(string queuePath)
        {
            var queue = _messageStore.GetQueue(queuePath);
            return _container.ResolveWithOverrides<InProcessQueueSender>(queue);
        }

        public INimbusMessageSender GetTopicSender(string topicPath)
        {
            var topic = _messageStore.GetTopic(topicPath);
            return _container.ResolveWithOverrides<InProcessTopicSender>(topic);
        }

        public INimbusMessageReceiver GetQueueReceiver(string queuePath)
        {
            var messageQueue = _messageStore.GetMessageQueue(queuePath);
            return _container.ResolveWithOverrides<InProcessQueueReceiver>(queuePath, messageQueue);
        }

        public INimbusMessageReceiver GetTopicReceiver(string topicPath, string subscriptionName)
        {
            var topic = _messageStore.GetTopic(topicPath);
            topic.Subscribe(subscriptionName);

            var fullyQualifiedSubscriptionPath = FullyQualifiedSubscriptionPath(topicPath, subscriptionName);
            var messageQueue = _messageStore.GetMessageQueue(fullyQualifiedSubscriptionPath);
            return _container.ResolveWithOverrides<InProcessQueueReceiver>(fullyQualifiedSubscriptionPath, messageQueue);
        }

        public static string FullyQualifiedSubscriptionPath(string topicPath, string subscriptionName)
        {
            var fullyQualifiedSubscriptionPath = topicPath + "." + subscriptionName;
            return fullyQualifiedSubscriptionPath;
        }
    }
}