using System.Threading.Tasks;
using Nimbus.Handlers;
using Nimbus.IntegrationTests.Tests.PoisonMessageTests.MessageContracts;
using Nimbus.Tests.Common.TestUtilities;

namespace Nimbus.IntegrationTests.Tests.PoisonMessageTests.EventHandlers
{
    public class DoesNotGoBangCompetingEventHandler : IHandleCompetingEvent<GoBangEvent>
    {
        public async Task Handle(GoBangEvent busEvent)
        {
            MethodCallCounter.RecordCall<DoesNotGoBangCompetingEventHandler>(h => h.Handle(busEvent));
        }
    }
}