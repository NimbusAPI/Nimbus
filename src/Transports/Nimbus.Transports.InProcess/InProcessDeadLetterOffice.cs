using System.Collections.Concurrent;
using System.Threading.Tasks;
using Nimbus.Configuration.Settings;
using Nimbus.Extensions;

namespace Nimbus.Transports.InProcess
{
    internal class InProcessDeadLetterOffice : IDeadLetterOffice
    {
        private readonly EnableDeadLetteringOnMessageExpirationSetting _enableDeadLetteringOnMessageExpiration;
        private readonly ConcurrentBag<NimbusMessage> _messages = new ConcurrentBag<NimbusMessage>();

        public InProcessDeadLetterOffice(EnableDeadLetteringOnMessageExpirationSetting enableDeadLetteringOnMessageExpiration)
        {
            _enableDeadLetteringOnMessageExpiration = enableDeadLetteringOnMessageExpiration;
        }

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
            return Task.Run(() =>
                            {
                                if (!_enableDeadLetteringOnMessageExpiration) return;

                                _messages.Add(message);
                            }).ConfigureAwaitFalse();
        }

        public Task<int> Count()
        {
            return Task.Run(() => _messages.Count).ConfigureAwaitFalse();
        }
    }
}