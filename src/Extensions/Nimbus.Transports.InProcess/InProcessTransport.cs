using System.Threading.Tasks;
using Nimbus.Configuration.PoorMansIocContainer;
using Nimbus.Filtering.Conditions;
using Nimbus.Infrastructure;
using Nimbus.Infrastructure.MessageSendersAndReceivers;
using Nimbus.Transports.InProcess.MessageSendersAndReceivers;
using Nimbus.Transports.InProcess.QueueManagement;

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

        public RetriesHandledBy RetriesHandledBy { get; } = RetriesHandledBy.Bus;

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
            var messageQueue = _messageStore.GetOrCreateMessageQueue(queuePath);
            return _container.ResolveWithOverrides<InProcessQueueReceiver>(queuePath, messageQueue);
        }

        public INimbusMessageReceiver GetTopicReceiver(string topicPath, string subscriptionName, IFilterCondition filter)
        {
            var subscription = new Subscription(topicPath, subscriptionName);
            return _container.ResolveWithOverrides<InProcessSubscriptionReceiver>(subscription);
        }

        public static string FullyQualifiedSubscriptionPath(string topicPath, string subscriptionName)
        {
            var fullyQualifiedSubscriptionPath = topicPath + "." + subscriptionName;
            return fullyQualifiedSubscriptionPath;
        }
    }
}