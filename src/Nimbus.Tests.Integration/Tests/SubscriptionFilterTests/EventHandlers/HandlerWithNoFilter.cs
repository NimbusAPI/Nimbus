using System;
using System.Threading.Tasks;
using Nimbus.InfrastructureContracts.Handlers;
using Nimbus.InfrastructureContracts.PropertyInjection;
using Nimbus.Tests.Common.TestUtilities;
using Nimbus.Tests.Integration.Tests.SubscriptionFilterTests.MessageContracts;

#pragma warning disable 4014

namespace Nimbus.Tests.Integration.Tests.SubscriptionFilterTests.EventHandlers
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