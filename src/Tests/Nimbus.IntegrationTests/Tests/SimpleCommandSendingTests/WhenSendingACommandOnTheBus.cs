using System;
using System.Linq;
using System.Threading.Tasks;
using Nimbus.Configuration;
using Nimbus.IntegrationTests.Tests.SimpleCommandSendingTests.MessageContracts;
using Nimbus.Tests.Common;
using Nimbus.Tests.Common.Extensions;
using Nimbus.Tests.Common.TestScenarioGeneration;
using Nimbus.Tests.Common.TestUtilities;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.IntegrationTests.Tests.SimpleCommandSendingTests
{
    [TestFixture]
    public class WhenSendingACommandOnTheBus : TestForBus
    {
        protected override async Task When()
        {
            var someCommand = new SomeCommand();
            await Bus.Send(someCommand);
            await TimeSpan.FromSeconds(TimeoutSeconds).WaitUntil(() => MethodCallCounter.AllReceivedMessages.Any());
        }

        [Test]
        [TestCaseSource(typeof (AllBusConfigurations<WhenSendingACommandOnTheBus>))]
        public async Task TheCommandBrokerShouldReceiveThatCommand(string testName, BusBuilderConfiguration busBuilderConfiguration)
        {
            await Given(busBuilderConfiguration);
            await When();

            MethodCallCounter.AllReceivedMessages.OfType<SomeCommand>().Count().ShouldBe(1);
        }

        [Test]
        [TestCaseSource(typeof (AllBusConfigurations<WhenSendingACommandOnTheBus>))]
        public async Task TheCorrectNumberOfTotalMessagesShouldHaveBeenObserved(string testName, BusBuilderConfiguration busBuilderConfiguration)
        {
            await Given(busBuilderConfiguration);
            await When();

            MethodCallCounter.AllReceivedMessages.Count().ShouldBe(1);
        }
    }
}