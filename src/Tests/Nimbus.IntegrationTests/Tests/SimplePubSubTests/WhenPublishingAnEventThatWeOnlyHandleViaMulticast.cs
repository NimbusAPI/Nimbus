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
    public class WhenPublishingAnEventThatWeOnlyHandleViaMulticast : TestForBus
    {
        public override async Task When()
        {
            var myEvent = new SomeEventWeOnlyHandleViaMulticast();
            await Bus.Publish(myEvent);

            TimeSpan.FromSeconds(5).SleepUntil(() => MethodCallCounter.AllReceivedMessages.Any());
        }

        [Test]
        public async Task TheMulticastEventBrokerShouldReceiveTheEvent()
        {
            await Given();
            await When();

            MethodCallCounter.ReceivedCallsWithAnyArg<SomeMulticastEventHandler>(mb => mb.Handle((SomeEventWeOnlyHandleViaMulticast) null))
                             .Count()
                             .ShouldBe(1);
        }

        [Test]
        public async Task TheCorrectNumberOfEventsOfThisTypeShouldHaveBeenObserved()
        {
            await Given();
            await When();

            MethodCallCounter.AllReceivedMessages
                             .OfType<SomeEventWeOnlyHandleViaMulticast>()
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