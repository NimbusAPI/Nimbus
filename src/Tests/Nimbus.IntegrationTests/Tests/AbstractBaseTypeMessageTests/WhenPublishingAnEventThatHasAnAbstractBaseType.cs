using System;
using System.Linq;
using System.Threading.Tasks;
using Nimbus.Configuration;
using Nimbus.IntegrationTests.Tests.AbstractBaseTypeMessageTests.Handlers;
using Nimbus.IntegrationTests.Tests.AbstractBaseTypeMessageTests.MessageContracts;
using Nimbus.Tests.Common.Extensions;
using Nimbus.Tests.Common.TestScenarioGeneration.ConfigurationSources;
using Nimbus.Tests.Common.TestScenarioGeneration.TestCaseSources;
using Nimbus.Tests.Common.TestUtilities;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.IntegrationTests.Tests.AbstractBaseTypeMessageTests
{
    public class WhenPublishingAnEventThatHasAnAbstractBaseType : TestForBus
    {
        protected override async Task When()
        {
            var busEvent = new SomeConcreteEventType();
            await Bus.Publish(busEvent);
            await TimeSpan.FromSeconds(TimeoutSeconds).WaitUntil(() => MethodCallCounter.AllReceivedMessages.Count() >= 2);
        }

        [Test]
        [TestCaseSource(typeof (AllBusConfigurations<WhenPublishingAnEventThatHasAnAbstractBaseType>))]
        public async Task TheCompetingEventBrokerShouldReceiveTheEvent(string testName, IConfigurationScenario<BusBuilderConfiguration> scenario)
        {
            await Given(scenario);
            await When();

            MethodCallCounter.ReceivedCallsWithAnyArg<SomeConcreteEventTypeCompetingHandler>(mb => mb.Handle(null))
                             .Count()
                             .ShouldBe(1);
        }

        [Test]
        [TestCaseSource(typeof (AllBusConfigurations<WhenPublishingAnEventThatHasAnAbstractBaseType>))]
        public async Task TheMulticastEventBrokerShouldReceiveTheEvent(string testName, IConfigurationScenario<BusBuilderConfiguration> scenario)
        {
            await Given(scenario);
            await When();

            MethodCallCounter.ReceivedCallsWithAnyArg<SomeConcreteEventTypeMulticastHandler>(mb => mb.Handle(null))
                             .Count()
                             .ShouldBe(1);
        }

        [Test]
        [TestCaseSource(typeof (AllBusConfigurations<WhenPublishingAnEventThatHasAnAbstractBaseType>))]
        public async Task TheCorrectNumberOfEventsOfThisTypeShouldHaveBeenObserved(string testName, IConfigurationScenario<BusBuilderConfiguration> scenario)
        {
            await Given(scenario);
            await When();

            MethodCallCounter.AllReceivedMessages
                             .OfType<SomeConcreteEventType>()
                             .Count()
                             .ShouldBe(2);
        }

        [Test]
        [TestCaseSource(typeof (AllBusConfigurations<WhenPublishingAnEventThatHasAnAbstractBaseType>))]
        public async Task TheCorrectNumberOfTotalMessagesShouldHaveBeenObserved(string testName, IConfigurationScenario<BusBuilderConfiguration> scenario)
        {
            await Given(scenario);
            await When();

            MethodCallCounter.AllReceivedMessages
                             .Count()
                             .ShouldBe(2);
        }
    }
}