using System.Threading.Tasks;
using Nimbus.Handlers;
using Nimbus.StressTests.ThreadStarvationTests.CommandHandlersSendingOtherCommands.MessageContracts;
using Nimbus.Tests.Common;

namespace Nimbus.StressTests.ThreadStarvationTests.CommandHandlersSendingOtherCommands.Handlers
{
    public class NoOpCommandHandler: IHandleCommand<NoOpCommand>
    {
        public async Task Handle(NoOpCommand busCommand)
        {
            MethodCallCounter.RecordCall<NoOpCommandHandler>(h => h.Handle(busCommand));
        }
    }
}