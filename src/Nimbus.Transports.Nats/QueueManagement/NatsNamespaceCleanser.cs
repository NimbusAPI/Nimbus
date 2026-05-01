using System.Threading.Tasks;
using Nimbus.Configuration;
using Nimbus.Transports.Nats.DeadLetterOffice;

namespace Nimbus.Transports.Nats.QueueManagement
{
    internal class NatsNamespaceCleanser : INamespaceCleanser
    {
        private readonly NatsDeadLetterOffice _deadLetterOffice;

        public NatsNamespaceCleanser(NatsDeadLetterOffice deadLetterOffice)
        {
            _deadLetterOffice = deadLetterOffice;
        }

        public Task RemoveAllExistingNamespaceElements()
        {
            _deadLetterOffice.Clear();
            return Task.CompletedTask;
        }
    }
}
