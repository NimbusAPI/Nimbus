using System;
using System.Linq;
using System.Threading.Tasks;
using Nimbus.IntegrationTests.Extensions;
using Nimbus.IntegrationTests.Tests.SimplePubSubTests.EventHandlers;
using Nimbus.IntegrationTests.Tests.SimplePubSubTests.MessageContracts;
using NUnit.Framework;
using Shouldly;

#pragma warning disable 4014

namespace Nimbus.IntegrationTests.Tests.SimplePubSubTests
{
    [TestFixture]
    public class WhenPublishingAnEventThatWeHandleViaCompetitionAndMulticast : TestForBus
    {
        public override async Task When()
        {
            await Bus.Publish(new SomeEventWeHandleViaMulticastAndCompetition());

            TimeSpan.FromSeconds(5).SleepUntil(() => MethodCallCounter.AllReceivedMessages.Count() >= 2);
        }

        [Test]
        public async Task TheCompetingEventBrokerShouldReceiveTheEvent()
        {
            await Given();
            await When();

            MethodCallCounter.ReceivedCallsWithAnyArg<SomeCompetingEventHandler>(mb => mb.Handle((SomeEventWeHandleViaMulticastAndCompetition) null))
                             .Count()
                             .ShouldBe(1);
        }

        [Test]
        public async Task TheMulticastEventBrokerShouldReceiveTheEvent()
        {
            await Given();
            await When();

            MethodCallCounter.ReceivedCallsWithAnyArg<SomeMulticastEventHandler>(mb => mb.Handle((SomeEventWeHandleViaMulticastAndCompetition) null))
                             .Count()
                             .ShouldBe(1);
        }

        [Test]
        public async Task TheCorrectNumberOfEventsOfThisTypeShouldHaveBeenObserved()
        {
            await Given();
            await When();

            MethodCallCounter.AllReceivedMessages
                             .OfType<SomeEventWeHandleViaMulticastAndCompetition>()
                             .Count()
                             .ShouldBe(2);
        }

        [Test]
        public async Task TheCorrectNumberOfTotalMessagesShouldHaveBeenObserved()
        {
            await Given();
            await When();

            MethodCallCounter.AllReceivedMessages
                             .Count()
                             .ShouldBe(2);
        }
    }
}