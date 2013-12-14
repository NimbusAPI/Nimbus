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
    public class WhenSendingACommandOnTheBus : TestForAllBuses
    {
        public override async Task When(ITestHarnessBusFactory busFactory)
        {
            var bus = busFactory.Create();

            var someCommand = new SomeCommand();
            await bus.Send(someCommand);
            TimeSpan.FromSeconds(5).SleepUntil(() => MethodCallCounter.AllReceivedMessages.Any());
        }

        [Test]
        [TestCaseSource("AllBusesTestCases")]
        public async Task TheCommandBrokerShouldReceiveThatCommand(ITestHarnessBusFactory busFactory)
        {
            await When(busFactory);

            MethodCallCounter.AllReceivedMessages.OfType<SomeCommand>().Count().ShouldBe(1);
        }

        [Test]
        [TestCaseSource("AllBusesTestCases")]
        public async void TheCorrectNumberOfTotalMessagesShouldHaveBeenObserved(ITestHarnessBusFactory busFactory)
        {
            await When(busFactory);

            MethodCallCounter.AllReceivedMessages.Count().ShouldBe(1);
        }
    }
}