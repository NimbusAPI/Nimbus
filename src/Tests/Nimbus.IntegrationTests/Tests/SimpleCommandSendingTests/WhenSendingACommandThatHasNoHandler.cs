using System.Threading.Tasks;
using Nimbus.Configuration;
using Nimbus.IntegrationTests.Tests.SimpleCommandSendingTests.MessageContracts;
using Nimbus.Tests.Common.TestScenarioGeneration;
using Nimbus.Tests.Common.TestScenarioGeneration.ConfigurationSources;
using Nimbus.Tests.Common.TestScenarioGeneration.ScenarioComposition;
using Nimbus.Tests.Common.TestScenarioGeneration.TestCaseSources;
using NUnit.Framework;

namespace Nimbus.IntegrationTests.Tests.SimpleCommandSendingTests
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
        public async Task NothingShouldGoBang(string testName, IConfigurationScenario<BusBuilderConfiguration> scenario)
        {
            await Given(scenario);
            await When();
        }
    }
}