using System.Collections.Concurrent;
using System.Threading.Tasks;
using Nimbus.Extensions;
using Nimbus.Infrastructure;
using Nimbus.Infrastructure.MessageSendersAndReceivers;

namespace Nimbus.Transports.InProcess
{
    internal class InProcessQueueSender : INimbusMessageSender
    {
        private readonly BlockingCollection<NimbusMessage> _queue;

        public InProcessQueueSender(BlockingCollection<NimbusMessage> queue)
        {
            _queue = queue;
        }

        public Task Send(NimbusMessage message)
        {
            return Task.Run(() => { _queue.Add(message); }).ConfigureAwaitFalse();
        }
    }
}