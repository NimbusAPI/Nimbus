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
    public class WhenPublishingAnEventThatWeOnlyHandleViaCompetition : TestForAllBuses
    {
        public override async Task When(ITestHarnessBusFactory busFactory)
        {
            var bus = busFactory.Create();

            var myEvent = new SomeEventWeOnlyHandleViaCompetition();
            await bus.Publish(myEvent);

            TimeSpan.FromSeconds(5).SleepUntil(() => MethodCallCounter.AllReceivedMessages.Any());
        }

        [Test]
        [TestCaseSource("AllBusesTestCases")]
        public async void TheCompetingEventBrokerShouldReceiveTheEvent(ITestHarnessBusFactory busFactory)
        {
            await When(busFactory);

            MethodCallCounter.ReceivedCallsWithAnyArg<SomeCompetingEventHandler>(mb => mb.Handle(null))
                             .Count()
                             .ShouldBe(1);
        }

        [Test]
        [TestCaseSource("AllBusesTestCases")]
        public async void TheMulticastEventBrokerShouldNotReceiveTheEvent(ITestHarnessBusFactory busFactory)
        {
            await When(busFactory);

            MethodCallCounter.ReceivedCallsWithAnyArg<SomeMulticastEventHandler>(mb => mb.Handle(null))
                             .Count()
                             .ShouldBe(0);
        }

        [Test]
        [TestCaseSource("AllBusesTestCases")]
        public async void TheCorrectNumberOfEventsOfThisTypeShouldHaveBeenObserved(ITestHarnessBusFactory busFactory)
        {
            await When(busFactory);

            MethodCallCounter.AllReceivedMessages
                             .OfType<SomeEventWeOnlyHandleViaCompetition>()
                             .Count()
                             .ShouldBe(1);
        }

        [Test]
        [TestCaseSource("AllBusesTestCases")]
        public async void TheCorrectNumberOfTotalMessagesShouldHaveBeenObserved(ITestHarnessBusFactory busFactory)
        {
            await When(busFactory);

            MethodCallCounter.AllReceivedMessages
                             .Count()
                             .ShouldBe(1);
        }
    }
}