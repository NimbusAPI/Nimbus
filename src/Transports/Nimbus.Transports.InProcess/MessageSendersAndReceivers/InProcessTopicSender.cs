using System.Threading.Tasks;
using Nimbus.Extensions;
using Nimbus.Infrastructure.MessageSendersAndReceivers;

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
            var clone = Clone(message);
            var topicQueue = _messageStore.GetMessageQueue(_topic.TopicPath);
            await topicQueue.Add(clone);

            _topic.SubscriptionNames
                  .Do(async subscriptionName =>
                            {
                                var messageClone = Clone(message);
                                var fullyQualifiedSubscriptionPath = InProcessTransport.FullyQualifiedSubscriptionPath(_topic.TopicPath, subscriptionName);
                                var subscriptionQueue = _messageStore.GetMessageQueue(fullyQualifiedSubscriptionPath);
                                await subscriptionQueue.Add(messageClone);
                            })
                  .Done();
        }

        private NimbusMessage Clone(NimbusMessage message)
        {
            var messageClone = (NimbusMessage) _serializer.Deserialize(_serializer.Serialize(message), typeof (NimbusMessage));
            return messageClone;
        }
    }
}