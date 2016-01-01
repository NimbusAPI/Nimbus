using System.Linq;
using System.Threading.Tasks;
using Nimbus.ConcurrentCollections;
using Nimbus.Extensions;
using Nimbus.Infrastructure.MessageSendersAndReceivers;
using Nimbus.Transports.InProcess.QueueManagement;

namespace Nimbus.Transports.InProcess.MessageSendersAndReceivers
{
    internal class InProcessTopicSender : INimbusMessageSender
    {
        private readonly ISerializer _serializer;
        private readonly Topic _topic;
        private readonly InProcessMessageStore _messageStore;

        public InProcessTopicSender(ISerializer serializer, Topic topic, InProcessMessageStore messageStore)
        {
            _serializer = serializer;
            _topic = topic;
            _messageStore = messageStore;
        }

        public async Task Send(NimbusMessage message)
        {
            await AddToCompetingSubscribersQueue(message);
            await AddToMulticastSubscriberQueues(message);
        }

        private async Task AddToCompetingSubscribersQueue(NimbusMessage message)
        {
            var clone = Clone(message);
            AsyncBlockingCollection<NimbusMessage> topicQueue;
            if (!_messageStore.TryGetExistingMessageQueue(_topic.TopicPath, out topicQueue)) return;
            await topicQueue.Add(clone);
        }

        private async Task AddToMulticastSubscriberQueues(NimbusMessage message)
        {
            await _topic.SubscriptionNames
                        .Select(subscriptionName =>
                                {
                                    var messageClone = Clone(message);
                                    var fullyQualifiedSubscriptionPath = InProcessTransport.FullyQualifiedSubscriptionPath(_topic.TopicPath, subscriptionName);
                                    var subscriptionQueue = _messageStore.GetOrCreateMessageQueue(fullyQualifiedSubscriptionPath);
                                    var task = subscriptionQueue.Add(messageClone);
                                    return task;
                                })
                        .WhenAll();
        }

        private NimbusMessage Clone(NimbusMessage message)
        {
            var messageClone = (NimbusMessage) _serializer.Deserialize(_serializer.Serialize(message), typeof (NimbusMessage));
            return messageClone;
        }
    }
}