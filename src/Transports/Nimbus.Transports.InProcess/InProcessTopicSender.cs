using System.Collections.Concurrent;
using System.Threading.Tasks;
using Nimbus.Extensions;
using Nimbus.Infrastructure;
using Nimbus.Infrastructure.MessageSendersAndReceivers;

namespace Nimbus.Transports.InProcess
{
    internal class InProcessTopicSender : INimbusMessageSender
    {
        private readonly InProcessMessageStore _messageStore;
        private readonly ConcurrentBag<string> _topic;

        public InProcessTopicSender(InProcessMessageStore messageStore, ConcurrentBag<string> topic)
        {
            _messageStore = messageStore;
            _topic = topic;
        }

        public Task Send(NimbusMessage message)
        {
            return Task.Run(() =>
                            {
                                _topic.ToArray()
                                      .Do(path => _messageStore.GetQueue(path).Add(message))
                                      .Done();
                            });
        }
    }
}