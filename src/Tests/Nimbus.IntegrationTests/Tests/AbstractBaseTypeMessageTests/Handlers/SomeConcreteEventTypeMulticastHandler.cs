using System;
using System.Threading.Tasks;
using Nimbus.Handlers;
using Nimbus.IntegrationTests.Tests.AbstractBaseTypeMessageTests.MessageContracts;
using Nimbus.PropertyInjection;
using Nimbus.Tests.Common.TestUtilities;

namespace Nimbus.IntegrationTests.Tests.AbstractBaseTypeMessageTests.Handlers
{
    public class SomeConcreteEventTypeMulticastHandler : IHandleMulticastEvent<SomeConcreteEventType>, IRequireBusId
    {
        public async Task Handle(SomeConcreteEventType busEvent)
        {
            MethodCallCounter.ForInstance(BusId).RecordCall<SomeConcreteEventTypeMulticastHandler>(ch => ch.Handle(busEvent));
        }

        public Guid BusId { get; set; }
    }
}