using System.Collections.Concurrent;
using System.Threading;
using Nimbus.Infrastructure;

namespace Nimbus.Transports.InProcess.MessageSendersAndReceivers
{
    internal class Queue
    {
        private readonly BlockingCollection<NimbusMessage> _messages = new BlockingCollection<NimbusMessage>();

        public void Add(NimbusMessage message)
        {
            _messages.Add(message);
        }

        public NimbusMessage Take(CancellationToken ct)
        {
            return _messages.Take(ct);
        }
    }
}