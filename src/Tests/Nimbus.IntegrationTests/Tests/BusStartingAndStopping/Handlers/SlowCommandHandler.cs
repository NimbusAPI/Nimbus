using System;
using System.Threading.Tasks;
using Nimbus.Handlers;
using Nimbus.IntegrationTests.Tests.BusStartingAndStopping.MessageContracts;

namespace Nimbus.IntegrationTests.Tests.BusStartingAndStopping.Handlers
{
    public class SlowCommandHandler : IHandleCommand<SlowCommand>
    {
        public async Task Handle(SlowCommand busCommand)
        {
            MethodCallCounter.RecordCall<SlowCommandHandler>(h => h.Handle(busCommand));

            await Task.Delay(TimeSpan.FromMilliseconds(500));
        }
    }
}