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
    public class WhenPublishingAnEventThatWeOnlyHandleViaCompetition : TestForBus
    {
        public override async Task When()
        {
            var myEvent = new SomeEventWeOnlyHandleViaCompetition();
            await Bus.Publish(myEvent);

            TimeSpan.FromSeconds(5).SleepUntil(() => MethodCallCounter.AllReceivedMessages.Any());
        }

        [Test]
        public async Task TheCompetingEventBrokerShouldReceiveTheEvent()
        {
            await Given();
            await When();

            MethodCallCounter.ReceivedCallsWithAnyArg<SomeCompetingEventHandler>(mb => mb.Handle((SomeEventWeOnlyHandleViaCompetition) null))
                             .Count()
                             .ShouldBe(1);
        }

        [Test]
        public async Task TheCorrectNumberOfEventsOfThisTypeShouldHaveBeenObserved()
        {
            await Given();
            await When();

            MethodCallCounter.AllReceivedMessages
                             .OfType<SomeEventWeOnlyHandleViaCompetition>()
                             .Count()
                             .ShouldBe(1);
        }

        [Test]
        public async Task TheCorrectNumberOfTotalMessagesShouldHaveBeenObserved()
        {
            await Given();
            await When();

            MethodCallCounter.AllReceivedMessages
                             .Count()
                             .ShouldBe(1);
        }
    }
}