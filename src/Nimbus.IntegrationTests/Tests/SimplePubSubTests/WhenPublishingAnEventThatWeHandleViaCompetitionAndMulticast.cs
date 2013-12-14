using System;
using System.Linq;
using System.Threading.Tasks;
using Nimbus.IntegrationTests.Extensions;
using Nimbus.IntegrationTests.InfrastructureContracts;
using Nimbus.IntegrationTests.Tests.SimplePubSubTests.EventHandlers;
using Nimbus.IntegrationTests.Tests.SimplePubSubTests.MessageContracts;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.IntegrationTests.Tests.SimplePubSubTests
{
    [TestFixture]
    public class WhenPublishingAnEventThatWeHandleViaCompetitionAndMulticast : TestForAllBuses
    {
        public override async Task When(ITestHarnessBusFactory busFactory)
        {
            var bus = busFactory.Create();

            await bus.Publish(new SomeEventWeHandleViaMulticastAndCompetition());

            TimeSpan.FromSeconds(5).SleepUntil(() => MethodCallCounter.AllReceivedMessages.Count() >= 2);
        }

        [Test]
        [TestCaseSource("AllBusesTestCases")]
        public async void TheCompetingEventBrokerShouldReceiveTheEvent(ITestHarnessBusFactory busFactory)
        {
            await When(busFactory);

            MethodCallCounter.ReceivedCallsWithAnyArg<SomeCompetingEventHandler>(mb => mb.Handle((SomeEventWeHandleViaMulticastAndCompetition) null))
                             .Count()
                             .ShouldBe(1);
        }

        [Test]
        [TestCaseSource("AllBusesTestCases")]
        public async void TheMulticastEventBrokerShouldReceiveTheEvent(ITestHarnessBusFactory busFactory)
        {
            await When(busFactory);

            MethodCallCounter.ReceivedCallsWithAnyArg<SomeMulticastEventHandler>(mb => mb.Handle((SomeEventWeHandleViaMulticastAndCompetition) null))
                             .Count()
                             .ShouldBe(1);
        }

        [Test]
        [TestCaseSource("AllBusesTestCases")]
        public async void TheCorrectNumberOfEventsOfThisTypeShouldHaveBeenObserved(ITestHarnessBusFactory busFactory)
        {
            await When(busFactory);

            MethodCallCounter.AllReceivedMessages
                             .OfType<SomeEventWeHandleViaMulticastAndCompetition>()
                             .Count()
                             .ShouldBe(2);
        }

        [Test]
        [TestCaseSource("AllBusesTestCases")]
        public async void TheCorrectNumberOfTotalMessagesShouldHaveBeenObserved(ITestHarnessBusFactory busFactory)
        {
            await When(busFactory);

            MethodCallCounter.AllReceivedMessages
                             .Count()
                             .ShouldBe(2);
        }
    }
}