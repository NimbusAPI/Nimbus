using System.Collections.Concurrent;
using System.Threading.Tasks;
using Nimbus.Extensions;
using Nimbus.Infrastructure;
using Nimbus.Infrastructure.MessageSendersAndReceivers;

namespace Nimbus.Transports.InProcess.MessageSendersAndReceivers
{
    internal class InProcessTopicSender : INimbusMessageSender
    {
        private readonly ISerializer _serializer;
        private readonly InProcessMessageStore _messageStore;
        private readonly ConcurrentBag<string> _topic;

        public InProcessTopicSender(ISerializer serializer, InProcessMessageStore messageStore, ConcurrentBag<string> topic)
        {
            _serializer = serializer;
            _messageStore = messageStore;
            _topic = topic;
        }

        public Task Send(NimbusMessage message)
        {
            return Task.Run(() =>
                            {
                                _topic.ToArray()
                                      .Do(path =>
                                          {
                                              var messageClone = (NimbusMessage) _serializer.Deserialize(_serializer.Serialize(message), typeof (NimbusMessage));
                                              var queue = _messageStore.GetQueue(path);
                                              queue.Add(messageClone);
                                          })
                                      .Done();
                            });
        }
    }
}