using System;
using System.Threading.Tasks;
using Nimbus.InfrastructureContracts;
using Nimbus.IntegrationTests.Tests.PoisonMessageTests.MessageContracts;

namespace Nimbus.IntegrationTests.Tests.PoisonMessageTests.CommandHandlers
{
    public class TestCommandHandler : IHandleCommand<TestCommand>
    {
        public async Task Handle(TestCommand busCommand)
        {
            throw new Exception("This handler is supposed to fail.");
        }
    }
}