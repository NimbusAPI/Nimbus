using System;
using System.Threading.Tasks;
using Nimbus.Handlers;
using Nimbus.IntegrationTests.Tests.SubscriptionFilterTests.MessageContracts;
using Nimbus.PropertyInjection;
using Nimbus.Tests.Common.TestUtilities;

#pragma warning disable 4014

namespace Nimbus.IntegrationTests.Tests.SubscriptionFilterTests.EventHandlers
{
    public class HandlerWithNoFilter : IHandleCompetingEvent<SomeEventAboutAParticularThing>, IRequireBusId
    {
        public async Task Handle(SomeEventAboutAParticularThing busEvent)
        {
            MethodCallCounter.ForInstance(BusId).RecordCall<HandlerWithNoFilter>(h => h.Handle(busEvent));
        }

        public Guid BusId { get; set; }
    }
}