using System.Threading.Tasks;
using Nimbus.InfrastructureContracts;
using Nimbus.UnitTests.MessageBrokerTests.CommandBrokerTests.MessageContracts;

namespace Nimbus.UnitTests.MessageBrokerTests.CommandBrokerTests.Handlers
{
    public class BrokerTestCommandHandler : IHandleCommand<FooCommand>
    {
        public async Task Handle(FooCommand busCommand)
        {
            MethodCallCounter.RecordCall<BrokerTestCommandHandler>(h => h.Handle(busCommand));
        }
    }
}