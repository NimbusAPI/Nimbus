using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Nimbus.IntegrationTests.Extensions;
using Nimbus.IntegrationTests.Tests.SimpleCommandSendingTests.MessageContracts;
using Shouldly;

namespace Nimbus.IntegrationTests.Tests.SimpleCommandSendingTests
{
    [TestFixture]
    public class WhenSendingACommandOnTheBus : SpecificationForBus
    {
        public override async Task WhenAsync()
        {
            var someCommand = new SomeCommand();
            await Subject.Send(someCommand);
            TimeSpan.FromSeconds(5).SleepUntil(() => MessageBroker.AllReceivedMessages.Any());
        }

        [Test]
        public void TheCommandBrokerShouldReceiveThatCommand()
        {
            MessageBroker.AllReceivedMessages.OfType<SomeCommand>().Count().ShouldBe(1);
        }

        [Test]
        public void TheCorrectNumberOfTotalMessagesShouldHaveBeenObserved()
        {
            MessageBroker.AllReceivedMessages.Count().ShouldBe(1);
        }
    }
}