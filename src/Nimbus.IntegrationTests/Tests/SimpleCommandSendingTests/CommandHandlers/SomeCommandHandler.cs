using Nimbus.InfrastructureContracts;
using Nimbus.IntegrationTests.Tests.SimpleCommandSendingTests.MessageContracts;

namespace Nimbus.IntegrationTests.Tests.SimpleCommandSendingTests.CommandHandlers
{
    public class SomeCommandHandler : IHandleCommand<SomeCommand>
    {
        public void Handle(SomeCommand busCommand)
        {
            MethodCallCounter.RecordCall<SomeCommandHandler>(ch => ch.Handle(busCommand));
        }
    }
}