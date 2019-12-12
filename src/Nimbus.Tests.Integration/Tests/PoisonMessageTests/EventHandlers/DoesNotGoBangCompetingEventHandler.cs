using System;
using System.Threading.Tasks;
using Nimbus.InfrastructureContracts.Handlers;
using Nimbus.InfrastructureContracts.PropertyInjection;
using Nimbus.Tests.Common.TestUtilities;
using Nimbus.Tests.Integration.Tests.PoisonMessageTests.MessageContracts;

namespace Nimbus.Tests.Integration.Tests.PoisonMessageTests.EventHandlers
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