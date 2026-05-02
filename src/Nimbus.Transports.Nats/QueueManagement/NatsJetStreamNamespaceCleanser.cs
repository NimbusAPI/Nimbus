using Nimbus.Configuration;
using Nimbus.InfrastructureContracts;
using Nimbus.Transports.Nats.ConnectionManagement;

namespace Nimbus.Transports.Nats.QueueManagement
{
    internal class NatsJetStreamNamespaceCleanser : INamespaceCleanser
    {
        private readonly IDeadLetterOffice _deadLetterOffice;
        private readonly NatsJetStreamContextFactory _jsContextFactory;

        public NatsJetStreamNamespaceCleanser(IDeadLetterOffice deadLetterOffice, NatsJetStreamContextFactory jsContextFactory)
        {
            _deadLetterOffice = deadLetterOffice;
            _jsContextFactory = jsContextFactory;
        }

        public async Task RemoveAllExistingNamespaceElements()
        {
            await _deadLetterOffice.Purge();
            await _jsContextFactory.DeleteAllStreamsAsync();
        }
    }
}
