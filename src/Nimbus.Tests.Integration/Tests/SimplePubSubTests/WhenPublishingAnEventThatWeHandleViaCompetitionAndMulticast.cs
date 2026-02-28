using System.Linq;
using System.Threading.Tasks;
using Nimbus.Configuration;
using Nimbus.Tests.Common.Extensions;
using Nimbus.Tests.Integration.Tests.SimplePubSubTests.EventHandlers;
using Nimbus.Tests.Integration.Tests.SimplePubSubTests.MessageContracts;
using Nimbus.Tests.Integration.TestScenarioGeneration.ScenarioComposition;
using Nimbus.Tests.Integration.TestScenarioGeneration.TestCaseSources;
using NUnit.Framework;
using Shouldly;

#pragma warning disable 4014

namespace Nimbus.Tests.Integration.Tests.SimplePubSubTests
{
    [TestFixture]
    public class WhenPublishingAnEventThatWeHandleViaCompetitionAndMulticast : TestForBus
    {
        protected override async Task When()
        {
            await Bus.Publish(new SomeEventWeHandleViaMulticastAndCompetition());

            await Timeout.WaitUntil(() => MethodCallCounter.AllReceivedMessages.Count() >= 2);
        }

        [Test]
        [TestCaseSource(typeof (AllBusConfigurations<WhenPublishingAnEventThatWeHandleViaCompetitionAndMulticast>))]
        public async Task Run(string testName, IConfigurationScenario<BusBuilderConfiguration> scenario)
        {
            await Given(scenario);
            await When();
            await Then();
        }

        [Then]
        public async Task TheCompetingEventBrokerShouldReceiveTheEvent()
        {
            MethodCallCounter.ReceivedCallsWithAnyArg<SomeCompetingEventHandler>(mb => mb.Handle((SomeEventWeHandleViaMulticastAndCompetition) null))
                             .Count()
                             .ShouldBe(1);
        }

        [Then]
        public async Task TheMulticastEventBrokerShouldReceiveTheEvent()
        {
            MethodCallCounter.ReceivedCallsWithAnyArg<SomeMulticastEventHandler>(mb => mb.Handle((SomeEventWeHandleViaMulticastAndCompetition) null))
                             .Count()
                             .ShouldBe(1);
        }

        [Then]
        public async Task TheCorrectNumberOfEventsOfThisTypeShouldHaveBeenObserved()
        {
            MethodCallCounter.AllReceivedMessages
                             .OfType<SomeEventWeHandleViaMulticastAndCompetition>()
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