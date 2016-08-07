using System;
using System.Threading.Tasks;
using Nimbus.Handlers;
using Nimbus.IntegrationTests.Tests.PoisonMessageTests.MessageContracts;
using Nimbus.Tests.Common.TestUtilities;

namespace Nimbus.IntegrationTests.Tests.PoisonMessageTests.EventHandlers
{
    public class GoBangCompetingEventHandler : IHandleCompetingEvent<GoBangEvent>
    {
        public async Task Handle(GoBangEvent busEvent)
        {
            MethodCallCounter.RecordCall<GoBangCompetingEventHandler>(h => h.Handle(busEvent));

            throw new Exception("This handler is supposed to fail.");
        }
    }
}
