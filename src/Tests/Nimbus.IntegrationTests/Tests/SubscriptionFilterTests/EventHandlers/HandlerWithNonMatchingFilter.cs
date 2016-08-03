using System.Threading.Tasks;
using Nimbus.Filtering;
using Nimbus.Filtering.Attributes;
using Nimbus.Handlers;
using Nimbus.IntegrationTests.Tests.SubscriptionFilterTests.MessageContracts;
using Nimbus.Tests.Common.TestUtilities;
using NUnit.Framework;

#pragma warning disable 4014

namespace Nimbus.IntegrationTests.Tests.SubscriptionFilterTests.EventHandlers
{
    [SubscriptionFilter(typeof(NonMatchingSubscriptionFilter))]
    public class HandlerWithNonMatchingFilter : IHandleCompetingEvent<SomeEventAboutAParticularThing>
    {
        public async Task Handle(SomeEventAboutAParticularThing busEvent)
        {
            MethodCallCounter.RecordCall<HandlerWithNonMatchingFilter>(h => h.Handle(busEvent));

            Assert.Fail("Oops! I should never have received that message!");
        }
    }
}