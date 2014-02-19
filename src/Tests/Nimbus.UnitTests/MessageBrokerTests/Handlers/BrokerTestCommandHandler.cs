using Nimbus.InfrastructureContracts;
using Nimbus.UnitTests.MessageBrokerTests.MessageContracts;

namespace Nimbus.UnitTests.MessageBrokerTests.Handlers
{
    public class BrokerTestCommandHandler : IHandleCommand<BrokerTestCommand>
    {
        public void Handle(BrokerTestCommand busCommand)
        {
            MethodCallCounter.RecordCall<BrokerTestCommandHandler>(h => h.Handle(busCommand));
        }
    }
}