using System;
using System.Linq;
using System.Threading.Tasks;
using Nimbus.IntegrationTests.Extensions;
using Nimbus.IntegrationTests.Tests.SimplePubSubTests.MessageContracts;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.IntegrationTests.Tests.SimplePubSubTests
{
    [TestFixture]
    public class WhenPublishingAnEventThatWeOnlyHandleViaMulticast : SpecificationForBus
    {
        public override async Task WhenAsync()
        {
            var myEvent = new SomeEventWeOnlyHandleViaMulticast();
            await Subject.Publish(myEvent);

            TimeSpan.FromSeconds(5).SleepUntil(() => MessageBroker.AllReceivedMessages.Any());
        }

        [Test]
        public void TheMulticastEventBrokerShouldReceiveTheEvent()
        {
            MessageBroker.ReceivedCallsWithAnyArg(mb => mb.PublishMulticast<SomeEventWeOnlyHandleViaMulticast>(null))
                         .Count()
                         .ShouldBe(1);
        }

        [Test]
        public void TheCompetingEventBrokerShouldNotReceiveTheEvent()
        {
            MessageBroker.ReceivedCallsWithAnyArg(mb => mb.PublishCompeting<SomeEventWeOnlyHandleViaMulticast>(null))
                         .Count()
                         .ShouldBe(0);
        }

        [Test]
        public void TheCorrectNumberOfEventsOfThisTypeShouldHaveBeenObserved()
        {
            MessageBroker.AllReceivedMessages
                         .OfType<SomeEventWeOnlyHandleViaMulticast>()
                         .Count()
                         .ShouldBe(1);
        }

        [Test]
        public void TheCorrectNumberOfTotalMessagesShouldHaveBeenObserved()
        {
            MessageBroker.AllReceivedMessages
                         .Count()
                         .ShouldBe(1);
        }
    }
}