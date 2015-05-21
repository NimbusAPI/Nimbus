using System;
using System.Threading.Tasks;
using Nimbus.Handlers;
using Nimbus.IntegrationTests.Tests.BusStartingAndStopping.MessageContracts;
using Nimbus.Tests.Common;

namespace Nimbus.IntegrationTests.Tests.BusStartingAndStopping.Handlers
{
#pragma warning disable 4014 // Because this call is not awaited, execution of the current method continues before the call is completed.
	public class SlowCommandHandler : IHandleCommand<SlowCommand>
    {
        public async Task Handle(SlowCommand busCommand)
        {
            MethodCallCounter.RecordCall<SlowCommandHandler>(h => h.Handle(busCommand));

            await Task.Delay(TimeSpan.FromMilliseconds(500));
        }
    }
}