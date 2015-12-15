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

        public InProcessTopicSender(ISerializer serializer, Topic topic)
        {
            _serializer = serializer;
            _topic = topic;
        }

        public Task Send(NimbusMessage message)
        {
            return Task.Run(() =>
                            {
                                _topic.SubscriptionQueues
                                      .Do(subscriptionQueue =>
                                          {
                                              var messageClone = (NimbusMessage) _serializer.Deserialize(_serializer.Serialize(message), typeof (NimbusMessage));
                                              subscriptionQueue.Add(messageClone);
                                          })
                                      .Done();
                            });
        }
    }
}