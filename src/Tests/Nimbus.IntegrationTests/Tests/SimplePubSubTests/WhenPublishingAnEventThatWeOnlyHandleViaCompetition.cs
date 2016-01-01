using System;
using System.Linq;
using System.Threading.Tasks;
using Nimbus.Configuration;
using Nimbus.IntegrationTests.Tests.SimplePubSubTests.EventHandlers;
using Nimbus.IntegrationTests.Tests.SimplePubSubTests.MessageContracts;
using Nimbus.Tests.Common.Extensions;
using Nimbus.Tests.Common.TestScenarioGeneration.ConfigurationSources;
using Nimbus.Tests.Common.TestScenarioGeneration.ScenarioComposition;
using Nimbus.Tests.Common.TestScenarioGeneration.TestCaseSources;
using Nimbus.Tests.Common.TestUtilities;
using NUnit.Framework;
using Shouldly;

#pragma warning disable 4014

namespace Nimbus.IntegrationTests.Tests.SimplePubSubTests
{
    [TestFixture]
    public class WhenPublishingAnEventThatWeOnlyHandleViaCompetition : TestForBus
    {
        protected override async Task When()
        {
            var myEvent = new SomeEventWeOnlyHandleViaCompetition();
            await Bus.Publish(myEvent);
        }

        [Test]
        [TestCaseSource(typeof (AllBusConfigurations<WhenPublishingAnEventThatWeOnlyHandleViaCompetition>))]
        public async Task TheCompetingEventBrokerShouldReceiveTheEvent(string testName, IConfigurationScenario<BusBuilderConfiguration> scenario)
        {
            await Given(scenario);
            await When();

            await TimeSpan.FromSeconds(TimeoutSeconds)
                          .WaitUntil(() =>
                                         MethodCallCounter.ReceivedCallsWithAnyArg<SomeCompetingEventHandler>(mb => mb.Handle((SomeEventWeOnlyHandleViaCompetition) null))
                                                          .Any());

            MethodCallCounter.ReceivedCallsWithAnyArg<SomeCompetingEventHandler>(mb => mb.Handle((SomeEventWeOnlyHandleViaCompetition) null))
                             .Count()
                             .ShouldBe(1);
        }

        [Test]
        [TestCaseSource(typeof (AllBusConfigurations<WhenPublishingAnEventThatWeOnlyHandleViaCompetition>))]
        public async Task TheCorrectNumberOfEventsOfThisTypeShouldHaveBeenObserved(string testName, IConfigurationScenario<BusBuilderConfiguration> scenario)
        {
            await Given(scenario);
            await When();

            await TimeSpan.FromSeconds(TimeoutSeconds)
                          .WaitUntil(() => MethodCallCounter.AllReceivedMessages
                                                            .OfType<SomeEventWeOnlyHandleViaCompetition>()
                                                            .Any());

            MethodCallCounter.AllReceivedMessages
                             .OfType<SomeEventWeOnlyHandleViaCompetition>()
                             .Count()
                             .ShouldBe(1);
        }

        [Test]
        [TestCaseSource(typeof (AllBusConfigurations<WhenPublishingAnEventThatWeOnlyHandleViaCompetition>))]
        public async Task TheCorrectNumberOfTotalMessagesShouldHaveBeenObserved(string testName, IConfigurationScenario<BusBuilderConfiguration> scenario)
        {
            await Given(scenario);
            await When();

            await TimeSpan.FromSeconds(TimeoutSeconds)
                          .WaitUntil(() => MethodCallCounter.AllReceivedMessages
                                                            .Any());

            MethodCallCounter.AllReceivedMessages
                             .Count()
                             .ShouldBe(1);
        }
    }
}