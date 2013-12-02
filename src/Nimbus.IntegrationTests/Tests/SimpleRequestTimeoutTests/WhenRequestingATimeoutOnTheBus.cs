using System;
using System.Linq;
using System.Threading.Tasks;
using Nimbus.IntegrationTests.Extensions;
using Nimbus.IntegrationTests.Tests.SimpleRequestTimeoutTests.MessageContracts;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.IntegrationTests.Tests.SimpleRequestTimeoutTests
{
    [TestFixture]
    public class WhenRequestingATimeoutOnTheBus : SpecificationForBus
    {
        public override async Task WhenAsync()
        {
            var someTimeout = new SomeTimeout();
            await Subject.Defer(TimeSpan.FromSeconds(30), someTimeout);
            TimeSpan.FromSeconds(35).SleepUntil(() => MessageBroker.AllReceivedMessages.Any());
        }

        [Test]
        public void TheTimeoutBrokerShouldReceiveThatTimeout()
        {
            MessageBroker.AllReceivedMessages.OfType<SomeTimeout>().Count().ShouldBe(1);
        }

        [Test]
        public void TheCorrectNumberOfTotalMessagesShouldHaveBeenObserved()
        {
            MessageBroker.AllReceivedMessages.Count().ShouldBe(1);
        }
    }
}