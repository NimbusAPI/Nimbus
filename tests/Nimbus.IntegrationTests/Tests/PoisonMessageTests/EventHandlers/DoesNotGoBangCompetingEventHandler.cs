using System;
using System.Threading.Tasks;
using Nimbus.Handlers;
using Nimbus.IntegrationTests.Tests.PoisonMessageTests.MessageContracts;
using Nimbus.PropertyInjection;
using Nimbus.Tests.Common.TestUtilities;

namespace Nimbus.IntegrationTests.Tests.PoisonMessageTests.EventHandlers
{
    public class DoesNotGoBangCompetingEventHandler : IHandleCompetingEvent<GoBangEvent>, IRequireBusId
    {
        public async Task Handle(GoBangEvent busEvent)
        {
            MethodCallCounter.ForInstance(BusId).RecordCall<DoesNotGoBangCompetingEventHandler>(h => h.Handle(busEvent));
        }

        public Guid BusId { get; set; }
    }
}