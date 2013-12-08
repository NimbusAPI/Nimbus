using Nimbus.InfrastructureContracts;
using Nimbus.IntegrationTests.Tests.BusBuilderTests.MessageContracts;

namespace Nimbus.IntegrationTests.Tests.BusBuilderTests.CommandHandlers
{
    public class SomeCommandHandler : IHandleCommand<SomeCommand>
    {
        public void Handle(SomeCommand busCommand)
        {
        }
    }
}