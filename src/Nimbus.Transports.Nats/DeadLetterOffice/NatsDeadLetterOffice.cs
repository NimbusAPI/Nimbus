using System.Collections.Concurrent;
using Nimbus.Extensions;
using Nimbus.InfrastructureContracts;

namespace Nimbus.Transports.Nats.DeadLetterOffice
{
    internal class NatsDeadLetterOffice : IDeadLetterOffice
    {
        private readonly ConcurrentQueue<NimbusMessage> _messages = new ConcurrentQueue<NimbusMessage>();

        public Task<NimbusMessage> Peek()
        {
            return Task.Run(() =>
            {
                _messages.TryPeek(out var result);
                return result!;
            }).ConfigureAwaitFalse();
        }

        public Task<NimbusMessage> Pop()
        {
            return Task.Run(() =>
            {
                _messages.TryDequeue(out var result);
                return result!;
            }).ConfigureAwaitFalse();
        }

        public Task Post(NimbusMessage message)
        {
            return Task.Run(() => _messages.Enqueue(message)).ConfigureAwaitFalse();
        }

        public Task<int> Count()
        {
            return Task.Run(() => _messages.Count).ConfigureAwaitFalse();
        }

        public Task Purge()
        {
            _messages.Clear();
            return Task.CompletedTask;
        }
    }
}
