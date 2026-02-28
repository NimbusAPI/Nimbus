using System.Linq;
using System.Threading.Tasks;
using Nimbus.Configuration;
using Nimbus.Tests.Common.Extensions;
using Nimbus.Tests.Integration.Tests.AbstractBaseTypeMessageTests.Handlers;
using Nimbus.Tests.Integration.Tests.AbstractBaseTypeMessageTests.MessageContracts;
using Nimbus.Tests.Integration.TestScenarioGeneration.ScenarioComposition;
using Nimbus.Tests.Integration.TestScenarioGeneration.ScenarioComposition.Filters;
using Nimbus.Tests.Integration.TestScenarioGeneration.TestCaseSources;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.Tests.Integration.Tests.AbstractBaseTypeMessageTests
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