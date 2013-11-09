using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Nimbus.IntegrationTests.Extensions;
using Nimbus.IntegrationTests.Tests.SimplePubSubTests.MessageContracts;
using Shouldly;

namespace Nimbus.IntegrationTests.Tests.SimplePubSubTests
{
    [TestFixture]
    public class WhenPublishingAnEventThatWeOnlyHandleViaCompetition : SpecificationForBus
    {
        public override async Task WhenAsync()
        {
            var myEvent = new SomeEventWeOnlyHandleViaCompetition();
            await Subject.Publish(myEvent);

            TimeSpan.FromSeconds(5).SleepUntil(() => MessageBroker.AllReceivedMessages.Any());
        }

        [Test]
        public void TheCompetingEventBrokerShouldReceiveTheEvent()
        {
            MessageBroker.ReceivedCallsWithAnyArg(mb => mb.PublishCompeting<SomeEventWeOnlyHandleViaCompetition>(null))
                         .Count()
                         .ShouldBe(1);
        }

        [Test]
        public void TheMulticastEventBrokerShouldNotReceiveTheEvent()
        {
            MessageBroker.ReceivedCallsWithAnyArg(mb => mb.PublishMulticast<SomeEventWeOnlyHandleViaCompetition>(null))
                         .Count()
                         .ShouldBe(0);
        }


        [Test]
        public void TheCorrectNumberOfEventsOfThisTypeShouldHaveBeenObserved()
        {
            MessageBroker.AllReceivedMessages
                         .OfType<SomeEventWeOnlyHandleViaCompetition>()
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