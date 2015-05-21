using System.Threading.Tasks;
using Nimbus.Handlers;
using Nimbus.StressTests.ThreadStarvationTests.CommandHandlersSendingOtherCommands.MessageContracts;
using Nimbus.Tests.Common;
using System.Threading;

namespace Nimbus.StressTests.ThreadStarvationTests.CommandHandlersSendingOtherCommands.Handlers
{
#pragma warning disable 1998 // This async method lacks 'await' operators and will run synchronously.
#pragma warning disable 4014 // Because this call is not awaited, execution of the current method continues before the call is completed.
	public class NoOpCommandHandler : IHandleCommand<NoOpCommand>
    {
        public async Task Handle(NoOpCommand busCommand)
        {
            MethodCallCounter.RecordCall<NoOpCommandHandler>(h => h.Handle(busCommand));
        }
    }
}