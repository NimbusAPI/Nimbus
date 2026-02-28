using System;
using System.Threading.Tasks;
using Nimbus.InfrastructureContracts.Handlers;
using Nimbus.InfrastructureContracts.PropertyInjection;
using Nimbus.Tests.Common.TestUtilities;
using Nimbus.Tests.Integration.Tests.PoisonMessageTests.MessageContracts;

namespace Nimbus.Tests.Integration.Tests.PoisonMessageTests.EventHandlers
{
    public class GoBangCompetingEventHandler : IHandleCompetingEvent<GoBangEvent>, IRequireBusId
    {
        public async Task Handle(GoBangEvent busEvent)
        {
            MethodCallCounter.ForInstance(BusId).RecordCall<GoBangCompetingEventHandler>(h => h.Handle(busEvent));

            throw new Exception("This handler is supposed to fail.");
        }

        public Guid BusId { get; set; }
    }
}
