using System;
using System.Threading.Tasks;
using Nimbus.Handlers;
using Nimbus.IntegrationTests.Tests.PoisonMessageTests.MessageContracts;

namespace Nimbus.IntegrationTests.Tests.PoisonMessageTests.CommandHandlers
{
    public class GoBangCommandHandler : IHandleCommand<GoBangCommand>
    {
        public async Task Handle(GoBangCommand busCommand)
        {
            throw new Exception("This handler is supposed to fail.");
        }
    }
}