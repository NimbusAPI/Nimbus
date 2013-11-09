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
    public class WhenPublishingAnEventThatWeHandleViaCompetitionAndMulticast : SpecificationForBus
    {
        public override async Task WhenAsync()
        {
            await Subject.Publish(new SomeEventWeHandleViaMulticastAndCompetition());

            TimeSpan.FromSeconds(5).SleepUntil(() => MessageBroker.AllReceivedMessages.Count() >= 2);
        }

        [Test]
        public void TheCompetingEventBrokerShouldReceiveTheEvent()
        {
            MessageBroker.ReceivedCallsWithAnyArg(mb => mb.PublishCompeting<SomeEventWeHandleViaMulticastAndCompetition>(null))
                         .Count()
                         .ShouldBe(1);
        }

        [Test]
        public void TheMulticastEventBrokerShouldReceiveTheEvent()
        {
            MessageBroker.ReceivedCallsWithAnyArg(mb => mb.PublishMulticast<SomeEventWeHandleViaMulticastAndCompetition>(null))
                         .Count()
                         .ShouldBe(1);
        }

        [Test]
        public void TheCorrectNumberOfEventsOfThisTypeShouldHaveBeenObserved()
        {
            MessageBroker.AllReceivedMessages
                         .OfType<SomeEventWeHandleViaMulticastAndCompetition>()
                         .Count()
                         .ShouldBe(2);
        }

        [Test]
        public void TheCorrectNumberOfTotalMessagesShouldHaveBeenObserved()
        {
            MessageBroker.AllReceivedMessages
                         .Count()
                         .ShouldBe(2);
        }
    }
}