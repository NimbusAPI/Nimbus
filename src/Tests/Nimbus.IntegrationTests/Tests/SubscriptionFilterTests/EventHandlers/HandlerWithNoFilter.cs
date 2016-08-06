using System.Threading.Tasks;
using Nimbus.Handlers;
using Nimbus.IntegrationTests.Tests.SubscriptionFilterTests.MessageContracts;
using Nimbus.Tests.Common.TestUtilities;

#pragma warning disable 4014

namespace Nimbus.IntegrationTests.Tests.SubscriptionFilterTests.EventHandlers
{
    public class HandlerWithNoFilter : IHandleCompetingEvent<SomeEventAboutAParticularThing>
    {
        public async Task Handle(SomeEventAboutAParticularThing busEvent)
        {
            MethodCallCounter.RecordCall<HandlerWithNoFilter>(h => h.Handle(busEvent));
        }
    }
}