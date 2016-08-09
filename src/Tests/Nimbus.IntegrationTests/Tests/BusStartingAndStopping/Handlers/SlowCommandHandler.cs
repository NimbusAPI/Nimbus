using System;
using System.Threading;
using System.Threading.Tasks;
using Nimbus.Handlers;
using Nimbus.IntegrationTests.Tests.BusStartingAndStopping.MessageContracts;
using Nimbus.PropertyInjection;
using Nimbus.Tests.Common.TestUtilities;

namespace Nimbus.IntegrationTests.Tests.BusStartingAndStopping.Handlers
{
    public class SlowCommandHandler : IHandleCommand<SlowCommand>, IRequireBusId
    {
        public static SemaphoreSlim HandlerSemaphore { get; private set; }

        static SlowCommandHandler()
        {
            Reset();
        }

        public static void Reset()
        {
            HandlerSemaphore = new SemaphoreSlim(0, int.MaxValue);
        }

        public async Task Handle(SlowCommand busCommand)
        {
            MethodCallCounter.ForInstance(BusId).RecordCall<SlowCommandHandler>(h => h.Handle(busCommand));

            await HandlerSemaphore.WaitAsync();
        }

        public Guid BusId { get; set; }
    }
}