using System.Threading.Tasks;
using Nimbus.Filtering;
using Nimbus.Filtering.Attributes;
using Nimbus.Handlers;
using Nimbus.IntegrationTests.Tests.SubscriptionFilterTests.MessageContracts;
using Nimbus.Tests.Common.TestUtilities;

#pragma warning disable 4014

namespace Nimbus.IntegrationTests.Tests.SubscriptionFilterTests.EventHandlers
{
    [SubscriptionFilter(typeof(MatchingSubscriptionFilter))]
    public class HandlerWithMatchingFilter : IHandleCompetingEvent<SomeEventAboutAParticularThing>
    {
        public async Task Handle(SomeEventAboutAParticularThing busEvent)
        {
            MethodCallCounter.RecordCall<HandlerWithMatchingFilter>(h => h.Handle(busEvent));
        }
    }
}