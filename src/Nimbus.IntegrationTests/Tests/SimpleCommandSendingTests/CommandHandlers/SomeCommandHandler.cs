using System;
using Nimbus.InfrastructureContracts;
using Nimbus.IntegrationTests.Tests.SimpleCommandSendingTests.MessageContracts;

namespace Nimbus.IntegrationTests.Tests.SimpleCommandSendingTests.CommandHandlers
{
    public class SomeCommandHandler : IHandleCommand<SomeCommand>
    {
        public void Handle(SomeCommand busCommand)
        {
            throw new NotImplementedException();
        }
    }
}