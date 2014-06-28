using System.Threading.Tasks;
using Nimbus.Handlers;
using Nimbus.Tests.Common;
using Nimbus.UnitTests.DependencyResolverTests.CommandHandlerResolutionTests.MessageContracts;

#pragma warning disable 4014

namespace Nimbus.UnitTests.DependencyResolverTests.CommandHandlerResolutionTests.Handlers
{
    public class BrokerTestCommandHandler : IHandleCommand<FooCommand>
    {
        public async Task Handle(FooCommand busCommand)
        {
            MethodCallCounter.RecordCall<BrokerTestCommandHandler>(h => h.Handle(busCommand));
        }
    }
}