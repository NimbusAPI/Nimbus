using System.Collections.Concurrent;
using System.Threading.Tasks;
using Nimbus.Extensions;
using Nimbus.InfrastructureContracts;

namespace Nimbus.Transports.InProcess.DeadLetterOffice
{
    internal class InProcessDeadLetterOffice : IDeadLetterOffice
    {
        private readonly ConcurrentBag<NimbusMessage> _messages = new ConcurrentBag<NimbusMessage>();

        public Task<NimbusMessage> Peek()
        {
            return Task.Run(() =>
                            {
                                NimbusMessage result;
                                _messages.TryPeek(out result);
                                return result;
                            }).ConfigureAwaitFalse();
        }

        public Task<NimbusMessage> Pop()
        {
            return Task.Run(() =>
                            {
                                NimbusMessage result;
                                _messages.TryTake(out result);
                                return result;
                            }).ConfigureAwaitFalse();
        }

        public Task Post(NimbusMessage message)
        {
            return Task.Run(() => _messages.Add(message)).ConfigureAwaitFalse();
        }

        public Task<int> Count()
        {
            return Task.Run(() => _messages.Count).ConfigureAwaitFalse();
        }
    }
}