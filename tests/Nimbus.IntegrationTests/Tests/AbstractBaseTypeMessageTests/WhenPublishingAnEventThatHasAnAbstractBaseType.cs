using System.Linq;
using System.Threading.Tasks;
using Nimbus.Configuration;
using Nimbus.IntegrationTests.Tests.AbstractBaseTypeMessageTests.Handlers;
using Nimbus.IntegrationTests.Tests.AbstractBaseTypeMessageTests.MessageContracts;
using Nimbus.IntegrationTests.TestScenarioGeneration.TestCaseSources;
using Nimbus.Tests.Common.Extensions;
using Nimbus.Tests.Common.TestScenarioGeneration.ScenarioComposition;
using Nimbus.Tests.Common.TestScenarioGeneration.ScenarioComposition.Filters;
using Nimbus.Tests.Common.TestScenarioGeneration.TestCaseSources;
using Nimbus.Tests.Common.TestUtilities;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.IntegrationTests.Tests.AbstractBaseTypeMessageTests
{
    [TestFixture]
    [FilterTestCasesBy(typeof(InProcessScenariosFilter))]
    public class WhenPublishingAnEventThatHasAnAbstractBaseType : TestForBus
    {
        protected override async Task When()
        {
            var busEvent = new SomeConcreteEventType();
            await Bus.Publish(busEvent);
            await Timeout.WaitUntil(() => MethodCallCounter.AllReceivedMessages.Count() >= 2);
        }

        [Test]
        [TestCaseSource(typeof (AllBusConfigurations<WhenPublishingAnEventThatHasAnAbstractBaseType>))]
        public async Task Run(string testName, IConfigurationScenario<BusBuilderConfiguration> scenario)
        {
            await Given(scenario);
            await When();
            await Then();
        }

        [Then]
        public async Task TheCompetingEventBrokerShouldReceiveTheEvent()
        {
            MethodCallCounter.ReceivedCallsWithAnyArg<SomeConcreteEventTypeCompetingHandler>(mb => mb.Handle(null))
                             .Count()
                             .ShouldBe(1);
        }

        [Then]
        public async Task TheMulticastEventBrokerShouldReceiveTheEvent()
        {
            MethodCallCounter.ReceivedCallsWithAnyArg<SomeConcreteEventTypeMulticastHandler>(mb => mb.Handle(null))
                             .Count()
                             .ShouldBe(1);
        }

        [Then]
        public async Task TheCorrectNumberOfEventsOfThisTypeShouldHaveBeenObserved()
        {
            MethodCallCounter.AllReceivedMessages
                             .OfType<SomeConcreteEventType>()
                             .Count()
                             .ShouldBe(2);
        }

        [Then]
        public async Task TheCorrectNumberOfTotalMessagesShouldHaveBeenObserved()
        {
            MethodCallCounter.AllReceivedMessages
                             .Count()
                             .ShouldBe(2);
        }
    }
}