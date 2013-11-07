using System;
using System.Linq;
using NSubstitute;
using NUnit.Framework;
using Nimbus.IntegrationTests.Extensions;
using Nimbus.IntegrationTests.Tests.SimplePubSubTests.MessageContracts;

namespace Nimbus.IntegrationTests.Tests.SimplePubSubTests
{
    [TestFixture]
    public class WhenPublishingAnEventThatWeOnlyHandleViaCompetition : SpecificationForBus
    {
        public override void When()
        {
            var myEvent = new SomeEventWeOnlyHandleViaCompetition();
            Subject.Publish(myEvent).Wait();

            TimeSpan.FromSeconds(10).SleepUntil(() => CompetingEventBroker.ReceivedCalls().Any());

            Subject.Stop();
        }

        [Test]
        public void TheCompetingEventBrokerShouldReceiveTheEvent()
        {
            CompetingEventBroker.Received().Publish(Arg.Any<SomeEventWeOnlyHandleViaCompetition>());
        }

        [Test]
        public void TheMulticastEventBrokerShouldNotReceiveTheEvent()
        {
            MulticastEventBroker.DidNotReceive().Publish(Arg.Any<SomeEventWeOnlyHandleViaCompetition>());
        }
    }
}