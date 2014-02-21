using System.Threading.Tasks;
using Nimbus.InfrastructureContracts;
using Nimbus.IntegrationTests.Tests.SimpleCommandSendingTests.MessageContracts;

#pragma warning disable 4014

namespace Nimbus.IntegrationTests.Tests.SimpleCommandSendingTests.CommandHandlers
{
    public class SomeCommandHandler : IHandleCommand<SomeCommand>
    {
        public async Task Handle(SomeCommand busCommand)
        {
            MethodCallCounter.RecordCall<SomeCommandHandler>(ch => ch.Handle(busCommand));
        }
    }
}
