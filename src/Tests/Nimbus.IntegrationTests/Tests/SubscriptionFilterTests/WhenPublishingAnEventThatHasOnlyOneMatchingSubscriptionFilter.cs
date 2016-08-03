using System.Linq;
using System.Threading.Tasks;
using Nimbus.Configuration;
using Nimbus.IntegrationTests.Tests.SubscriptionFilterTests.EventHandlers;
using Nimbus.IntegrationTests.Tests.SubscriptionFilterTests.MessageContracts;
using Nimbus.Tests.Common.Extensions;
using Nimbus.Tests.Common.TestScenarioGeneration.ScenarioComposition;
using Nimbus.Tests.Common.TestScenarioGeneration.TestCaseSources;
using Nimbus.Tests.Common.TestUtilities;
using NUnit.Framework;
using Shouldly;

#pragma warning disable 4014

namespace Nimbus.IntegrationTests.Tests.SubscriptionFilterTests
{
    public class WhenPublishingAnEventThatHasOnlyOneMatchingSubscriptionFilter : TestForBus
    {
        private SomeEventAboutAParticularThing _busEvent;

        protected override async Task When()
        {
            _busEvent = new SomeEventAboutAParticularThing
                        {
                            ThingId = MatchingSubscriptionFilter.MySpecialThingId
                        };

            await Bus.Publish(_busEvent);

            await Timeout.WaitUntil(() => MethodCallCounter.AllReceivedMessages.Count() >= 2);
        }

        [Test]
        [TestCaseSource(typeof(AllBusConfigurations<WhenPublishingAnEventThatHasOnlyOneMatchingSubscriptionFilter>))]
        public async Task Run(string testName, IConfigurationScenario<BusBuilderConfiguration> scenario)
        {
            await Given(scenario);
            await When();
            await Then();
        }

        [Then]
        public async Task TheHandlerWithTheMatchingExpressionShouldReceiveTheEvent()
        {
            MethodCallCounter.ReceivedCallsWithAnyArg<HandlerWithMatchingFilter>(h => h.Handle(_busEvent))
                             .Count()
                             .ShouldBe(1);
        }

        [Then]
        public async Task TheHandlerWithTheNonMatchingSubscriptionShouldNotReceiveTheEvent()
        {
            MethodCallCounter.ReceivedCallsWithAnyArg<HandlerWithNonMatchingFilter>(h => h.Handle(_busEvent))
                             .Count()
                             .ShouldBe(0);
        }

        [Then]
        public async Task HandlersWithNoFilterShouldReceiveTheEvent()
        {
            MethodCallCounter.ReceivedCallsWithAnyArg<HandlerWithNoFilter>(h => h.Handle(_busEvent))
                             .Count()
                             .ShouldBe(1);
        }
    }
}