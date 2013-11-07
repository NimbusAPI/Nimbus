using System;
using System.Linq;
using NSubstitute;
using NUnit.Framework;
using Nimbus.IntegrationTests.EventTests.MessageContracts;
using Nimbus.IntegrationTests.Extensions;

namespace Nimbus.IntegrationTests.EventTests
{
    [TestFixture]
    public class WhenPublishingAnEventThatWeOnlyHandleViaCompetition : SpecificationForBus
    {
        public override void When()
        {
            var myEvent = new SomeEventWeOnlyHandleViaCompetition();
            Subject.Publish(myEvent).Wait();

            TimeSpan.FromSeconds(5).SleepUntil(() => _competingEventBroker.ReceivedCalls().Any());

            Subject.Stop();
        }

        [Test]
        public void TheCompetingEventBrokerShouldReceiveTheEvent()
        {
            _competingEventBroker.Received().Publish(Arg.Any<SomeEventWeOnlyHandleViaCompetition>());
        }

        [Test]
        public void TheMulticastEventBrokerShouldNotReceiveTheEvent()
        {
            _multicastEventBroker.DidNotReceive().Publish(Arg.Any<SomeEventWeOnlyHandleViaCompetition>());
        }
    }
}