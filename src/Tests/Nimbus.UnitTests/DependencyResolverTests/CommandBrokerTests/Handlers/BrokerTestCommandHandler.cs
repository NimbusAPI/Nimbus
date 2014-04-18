using System.Threading.Tasks;
using Nimbus.Handlers;
using Nimbus.UnitTests.DependencyResolverTests.CommandBrokerTests.MessageContracts;

#pragma warning disable 4014

namespace Nimbus.UnitTests.DependencyResolverTests.CommandBrokerTests.Handlers
{
    public class BrokerTestCommandHandler : IHandleCommand<FooCommand>
    {
        public async Task Handle(FooCommand busCommand)
        {
            MethodCallCounter.RecordCall<BrokerTestCommandHandler>(h => h.Handle(busCommand));
        }
    }
}