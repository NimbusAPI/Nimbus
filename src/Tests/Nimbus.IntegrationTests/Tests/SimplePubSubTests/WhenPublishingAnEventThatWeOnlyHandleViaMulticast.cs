using System;
using System.Linq;
using System.Threading.Tasks;
using Nimbus.IntegrationTests.Extensions;
using Nimbus.IntegrationTests.Tests.SimplePubSubTests.EventHandlers;
using Nimbus.IntegrationTests.Tests.SimplePubSubTests.MessageContracts;
using Nimbus.Tests.Common;
using NUnit.Framework;
using Shouldly;

#pragma warning disable 4014

namespace Nimbus.IntegrationTests.Tests.SimplePubSubTests
{
    [TestFixture]
    public class WhenPublishingAnEventThatWeOnlyHandleViaMulticast : TestForBus
    {
        protected override async Task When()
        {
            var myEvent = new SomeEventWeOnlyHandleViaMulticast();
            await Bus.Publish(myEvent);

            await TimeSpan.FromSeconds(5).WaitUntil(() => MethodCallCounter.AllReceivedMessages.Any());
        }

        [Test]
        public async Task TheMulticastEventBrokerShouldReceiveTheEvent()
        {
            MethodCallCounter.ReceivedCallsWithAnyArg<SomeMulticastEventHandler>(mb => mb.Handle((SomeEventWeOnlyHandleViaMulticast) null))
                             .Count()
                             .ShouldBe(1);
        }

        [Test]
        public async Task TheCorrectNumberOfEventsOfThisTypeShouldHaveBeenObserved()
        {
            MethodCallCounter.AllReceivedMessages
                             .OfType<SomeEventWeOnlyHandleViaMulticast>()
                             .Count()
                             .ShouldBe(1);
        }

        [Test]
        public async Task TheCorrectNumberOfTotalMessagesShouldHaveBeenObserved()
        {
            MethodCallCounter.AllReceivedMessages
                             .Count()
                             .ShouldBe(1);
        }
    }
}