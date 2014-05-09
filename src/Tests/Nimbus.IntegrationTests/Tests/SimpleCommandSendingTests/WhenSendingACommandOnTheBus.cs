using System;
using System.Linq;
using System.Threading.Tasks;
using Nimbus.IntegrationTests.Extensions;
using Nimbus.IntegrationTests.Tests.SimpleCommandSendingTests.MessageContracts;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.IntegrationTests.Tests.SimpleCommandSendingTests
{
    [TestFixture]
    [Timeout(15*1000)]
    public class WhenSendingACommandOnTheBus : TestForBus
    {
        public override async Task When()
        {
            var someCommand = new SomeCommand();
            await Bus.Send(someCommand);
            TimeSpan.FromSeconds(10).SleepUntil(() => MethodCallCounter.AllReceivedMessages.Any());
        }

        [Test]
        public async Task TheCommandBrokerShouldReceiveThatCommand()
        {
            await Given();
            await When();

            MethodCallCounter.AllReceivedMessages.OfType<SomeCommand>().Count().ShouldBe(1);
        }

        [Test]
        public async Task TheCorrectNumberOfTotalMessagesShouldHaveBeenObserved()
        {
            await Given();
            await When();

            MethodCallCounter.AllReceivedMessages.Count().ShouldBe(1);
        }
    }
}