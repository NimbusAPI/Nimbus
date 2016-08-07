using System;
using System.Threading.Tasks;
using Nimbus.Handlers;
using Nimbus.IntegrationTests.Tests.SimplePubSubTests.MessageContracts;
using Nimbus.PropertyInjection;
using Nimbus.Tests.Common.TestUtilities;

#pragma warning disable 4014

namespace Nimbus.IntegrationTests.Tests.SimplePubSubTests.EventHandlers
{
    public class SomeCompetingEventHandler : IHandleCompetingEvent<SomeEventWeOnlyHandleViaCompetition>,
                                             IHandleCompetingEvent<SomeEventWeHandleViaMulticastAndCompetition>,
                                             IRequireBusId
    {
        public async Task Handle(SomeEventWeOnlyHandleViaCompetition busEvent)
        {
            MethodCallCounter.ForInstance(BusId).RecordCall<SomeCompetingEventHandler>(h => h.Handle(busEvent));
        }

        public async Task Handle(SomeEventWeHandleViaMulticastAndCompetition busEvent)
        {
            MethodCallCounter.ForInstance(BusId).RecordCall<SomeCompetingEventHandler>(h => h.Handle(busEvent));
        }

        public Guid BusId { get; set; }
    }
}