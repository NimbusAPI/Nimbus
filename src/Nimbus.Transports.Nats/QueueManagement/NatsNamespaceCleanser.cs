using System.Threading.Tasks;
using Nimbus.Configuration;
using Nimbus.InfrastructureContracts;

namespace Nimbus.Transports.Nats.QueueManagement
{
    internal class NatsNamespaceCleanser : INamespaceCleanser
    {
        private readonly IDeadLetterOffice _deadLetterOffice;

        public NatsNamespaceCleanser(IDeadLetterOffice deadLetterOffice)
        {
            _deadLetterOffice = deadLetterOffice;
        }

        public Task RemoveAllExistingNamespaceElements()
        {
            return _deadLetterOffice.Purge();
        }
    }
}
