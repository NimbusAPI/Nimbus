using System;
using System.Threading.Tasks;
using Nimbus.InfrastructureContracts.Handlers;
using Nimbus.InfrastructureContracts.PropertyInjection;
using Nimbus.Tests.Common.TestUtilities;
using Nimbus.Tests.Integration.Tests.AbstractBaseTypeMessageTests.MessageContracts;

namespace Nimbus.Tests.Integration.Tests.AbstractBaseTypeMessageTests.Handlers
{
    public class SomeConcreteEventTypeCompetingHandler : IHandleCompetingEvent<SomeConcreteEventType>, IRequireBusId
    {
        public async Task Handle(SomeConcreteEventType busEvent)
        {
            MethodCallCounter.ForInstance(BusId).RecordCall<SomeConcreteEventTypeCompetingHandler>(ch => ch.Handle(busEvent));
        }

        public Guid BusId { get; set; }
    }
}