using System;
using System.Threading.Tasks;
using Nimbus.Handlers;
using Nimbus.IntegrationTests.Tests.PoisonMessageTests.MessageContracts;
using Nimbus.Tests.Common;

namespace Nimbus.IntegrationTests.Tests.PoisonMessageTests.CommandHandlers
{
#pragma warning disable 4014 // Because this call is not awaited, execution of the current method continues before the call is completed.
	public class GoBangCommandHandler : IHandleCommand<GoBangCommand>
    {
        public async Task Handle(GoBangCommand busCommand)
        {
            MethodCallCounter.RecordCall<GoBangCommandHandler>(h => h.Handle(busCommand));

            throw new Exception("This handler is supposed to fail.");
        }
    }
}