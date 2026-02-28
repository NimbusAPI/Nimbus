using System.Linq;
using System.Threading.Tasks;
using Nimbus.Configuration;
using Nimbus.Tests.Common.Extensions;
using Nimbus.Tests.Integration.Tests.SimpleCommandSendingTests.MessageContracts;
using Nimbus.Tests.Integration.TestScenarioGeneration.ScenarioComposition;
using Nimbus.Tests.Integration.TestScenarioGeneration.TestCaseSources;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.Tests.Integration.Tests.SimpleCommandSendingTests
{
    [TestFixture]
    public class WhenSendingACommandOnTheBus : TestForBus
    {
        protected override async Task When()
        {
            var someCommand = new SomeCommand();
            await Bus.Send(someCommand);
            await Timeout.WaitUntil(() => MethodCallCounter.AllReceivedMessages.Any());
        }

        [Test]
        [TestCaseSource(typeof (AllBusConfigurations<WhenSendingACommandOnTheBus>))]
        public async Task Run(string testName, IConfigurationScenario<BusBuilderConfiguration> scenario)
        {
            await Given(scenario); 
            await When();
            await Then();
        }

        [Then]
        public async Task TheCommandBrokerShouldReceiveThatCommand()
        {
            MethodCallCounter.AllReceivedMessages.OfType<SomeCommand>().Count().ShouldBe(1);
        }

        [Then]
        public async Task TheCorrectNumberOfTotalMessagesShouldHaveBeenObserved()
        {
            MethodCallCounter.AllReceivedMessages.Count().ShouldBe(1);
        }
    }
}