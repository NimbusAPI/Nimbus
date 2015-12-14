using System.Collections.Concurrent;
using System.Threading.Tasks;
using Nimbus.Extensions;
using Nimbus.Infrastructure;
using Nimbus.Infrastructure.MessageSendersAndReceivers;

namespace Nimbus.Transports.InProcess.MessageSendersAndReceivers
{
    internal class InProcessQueueSender : INimbusMessageSender
    {
        private readonly ISerializer _serializer;
        private readonly BlockingCollection<NimbusMessage> _queue;

        public InProcessQueueSender(ISerializer serializer, BlockingCollection<NimbusMessage> queue)
        {
            _serializer = serializer;
            _queue = queue;
        }

        public Task Send(NimbusMessage message)
        {
            return Task.Run(() =>
                            {
                                var messageClone = (NimbusMessage) _serializer.Deserialize(_serializer.Serialize(message), typeof (NimbusMessage));
                                _queue.Add(messageClone);
                            }).ConfigureAwaitFalse();
        }
    }
}