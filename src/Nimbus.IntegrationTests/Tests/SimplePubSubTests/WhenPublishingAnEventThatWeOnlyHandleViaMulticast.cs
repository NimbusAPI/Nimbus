using System;
using System.Linq;
using NSubstitute;
using NUnit.Framework;
using Nimbus.IntegrationTests.Extensions;
using Nimbus.IntegrationTests.Tests.SimplePubSubTests.MessageContracts;

namespace Nimbus.IntegrationTests.Tests.SimplePubSubTests
{
    [TestFixture]
    public class WhenPublishingAnEventThatWeOnlyHandleViaMulticast : SpecificationForBus
    {
        public override void When()
        {
            var myEvent = new SomeEventWeOnlyHandleViaMulticast();
            Subject.Publish(myEvent).Wait();

            TimeSpan.FromSeconds(5).SleepUntil(() => MulticastEventBroker.ReceivedCalls().Any());

            Subject.Stop();
        }

        [Test]
        public void TheMulticastEventBrokerShouldReceiveTheEvent()
        {
            MulticastEventBroker.Received().Publish(Arg.Any<SomeEventWeOnlyHandleViaMulticast>());
        }

        [Test]
        public void TheCompetingEventBrokerShouldNotReceiveTheEvent()
        {
            CompetingEventBroker.DidNotReceive().Publish(Arg.Any<SomeEventWeOnlyHandleViaMulticast>());
        }
    }
}