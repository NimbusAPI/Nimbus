using System;
using System.Linq;
using System.Threading.Tasks;
using Nimbus.IntegrationTests.Extensions;
using Nimbus.IntegrationTests.InfrastructureContracts;
using Nimbus.IntegrationTests.Tests.SimplePubSubTests.EventHandlers;
using Nimbus.IntegrationTests.Tests.SimplePubSubTests.MessageContracts;
using NUnit.Framework;
using Shouldly;

#pragma warning disable 4014

namespace Nimbus.IntegrationTests.Tests.SimplePubSubTests
{
    [TestFixture]
    public class WhenPublishingAnEventThatWeHandleViaCompetitionAndMulticast : TestForAllBuses
    {
        public override async Task When()
        {
            await Bus.Publish(new SomeEventWeHandleViaMulticastAndCompetition());

            TimeSpan.FromSeconds(5).SleepUntil(() => MethodCallCounter.AllReceivedMessages.Count() >= 2);
        }

        [Test]
        [TestCaseSource("AllBusesTestCases")]
        public async Task TheCompetingEventBrokerShouldReceiveTheEvent(ITestHarnessBusFactory busFactory)
        {
            await Given(busFactory);
            await When();

            MethodCallCounter.ReceivedCallsWithAnyArg<SomeCompetingEventHandler>(mb => mb.Handle((SomeEventWeHandleViaMulticastAndCompetition) null))
                             .Count()
                             .ShouldBe(1);
        }

        [Test]
        [TestCaseSource("AllBusesTestCases")]
        public async Task TheMulticastEventBrokerShouldReceiveTheEvent(ITestHarnessBusFactory busFactory)
        {
            await Given(busFactory);
            await When();

            MethodCallCounter.ReceivedCallsWithAnyArg<SomeMulticastEventHandler>(mb => mb.Handle((SomeEventWeHandleViaMulticastAndCompetition) null))
                             .Count()
                             .ShouldBe(1);
        }

        [Test]
        [TestCaseSource("AllBusesTestCases")]
        public async Task TheCorrectNumberOfEventsOfThisTypeShouldHaveBeenObserved(ITestHarnessBusFactory busFactory)
        {
            await Given(busFactory);
            await When();

            MethodCallCounter.AllReceivedMessages
                             .OfType<SomeEventWeHandleViaMulticastAndCompetition>()
                             .Count()
                             .ShouldBe(2);
        }

        [Test]
        [TestCaseSource("AllBusesTestCases")]
        public async Task TheCorrectNumberOfTotalMessagesShouldHaveBeenObserved(ITestHarnessBusFactory busFactory)
        {
            await Given(busFactory);
            await When();

            MethodCallCounter.AllReceivedMessages
                             .Count()
                             .ShouldBe(2);
        }
    }
}