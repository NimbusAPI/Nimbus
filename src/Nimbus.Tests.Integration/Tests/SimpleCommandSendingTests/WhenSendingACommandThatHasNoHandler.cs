using System.Threading.Tasks;
using Nimbus.Configuration;
using Nimbus.Tests.Integration.Tests.SimpleCommandSendingTests.MessageContracts;
using Nimbus.Tests.Integration.TestScenarioGeneration.ScenarioComposition;
using Nimbus.Tests.Integration.TestScenarioGeneration.TestCaseSources;
using NUnit.Framework;

namespace Nimbus.Tests.Integration.Tests.SimpleCommandSendingTests
{
    [TestFixture]
    public class WhenSendingACommandThatHasNoHandler : TestForBus
    {
        protected override async Task When()
        {
            var someCommand = new SomeCommandThatHasNoHandler();
            await Bus.Send(someCommand);
        }

        [Test]
        [TestCaseSource(typeof (AllBusConfigurations<WhenSendingACommandThatHasNoHandler>))]
        public async Task Run(string testName, IConfigurationScenario<BusBuilderConfiguration> scenario)
        {
            await Given(scenario);
            await When();
            await Then();
        }

        [Then]
        public async Task NothingShouldGoBang()
        {
        }
    }
}