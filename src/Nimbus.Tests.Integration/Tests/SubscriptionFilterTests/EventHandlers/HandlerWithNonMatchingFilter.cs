using System;
using System.Threading.Tasks;
using Nimbus.InfrastructureContracts.Filtering.Attributes;
using Nimbus.InfrastructureContracts.Handlers;
using Nimbus.InfrastructureContracts.PropertyInjection;
using Nimbus.Tests.Common.TestUtilities;
using Nimbus.Tests.Integration.Tests.SubscriptionFilterTests.MessageContracts;
using NUnit.Framework;

#pragma warning disable 4014

namespace Nimbus.Tests.Integration.Tests.SubscriptionFilterTests.EventHandlers
{
    [SubscriptionFilter(typeof(NonMatchingSubscriptionFilter))]
    public class HandlerWithNonMatchingFilter : IHandleCompetingEvent<SomeEventAboutAParticularThing>, IRequireBusId
    {
        public async Task Handle(SomeEventAboutAParticularThing busEvent)
        {
            MethodCallCounter.ForInstance(BusId).RecordCall<HandlerWithNonMatchingFilter>(h => h.Handle(busEvent));

            Assert.Fail("Oops! I should never have received that message!");
        }

        public Guid BusId { get; set; }
    }
}