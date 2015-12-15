using System.Threading.Tasks;
using Nimbus.Extensions;
using Nimbus.Infrastructure;
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

        public Task Send(NimbusMessage message)
        {
            return Task.Run(() =>
                            {
                                _topic.SubscriptionNames
                                      .Do(subscriptionName =>
                                          {
                                              var messageClone = (NimbusMessage) _serializer.Deserialize(_serializer.Serialize(message), typeof (NimbusMessage));
                                              var fullyQualifiedSubscriptionPath = InProcessTransport.FullyQualifiedSubscriptionPath(_topic.TopicPath, subscriptionName);
                                              var subscriptionQueue = _messageStore.GetMessageQueue(fullyQualifiedSubscriptionPath);
                                              subscriptionQueue.Add(messageClone);
                                          })
                                      .Done();
                            });
        }
    }
}