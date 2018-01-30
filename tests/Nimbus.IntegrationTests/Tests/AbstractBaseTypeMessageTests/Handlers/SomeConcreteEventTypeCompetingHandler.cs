using System;
using System.Threading.Tasks;
using Nimbus.Handlers;
using Nimbus.IntegrationTests.Tests.AbstractBaseTypeMessageTests.MessageContracts;
using Nimbus.PropertyInjection;
using Nimbus.Tests.Common;
using Nimbus.Tests.Common.TestUtilities;

namespace Nimbus.IntegrationTests.Tests.AbstractBaseTypeMessageTests.Handlers
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