using System;
using System.Linq;
using NSubstitute;
using NUnit.Framework;
using Nimbus.IntegrationTests.Extensions;
using Nimbus.IntegrationTests.Tests.SimplePubSubTests.MessageContracts;

namespace Nimbus.IntegrationTests.Tests.SimplePubSubTests
{
    [TestFixture]
    public class WhenPublishingAnEventThatWeHandleViaCompetitionAndMulticast : SpecificationForBus
    {
        public override void When()
        {
            var myEvent = new SomeEventWeHandleViaMulticastAndCompetition();
            Subject.Publish(myEvent).Wait();

            TimeSpan.FromSeconds(10).SleepUntil(() => CompetingEventBroker.ReceivedCalls().Any());

            Subject.Stop();
        }

        [Test]
        public void TheCompetingEventBrokerShouldReceiveTheEvent()
        {
            CompetingEventBroker.Received().Publish(Arg.Any<SomeEventWeHandleViaMulticastAndCompetition>());
        }

        [Test]
        public void TheMulticastEventBrokerShouldReceiveTheEvent()
        {
            MulticastEventBroker.Received().Publish(Arg.Any<SomeEventWeHandleViaMulticastAndCompetition>());
        }
    }
}