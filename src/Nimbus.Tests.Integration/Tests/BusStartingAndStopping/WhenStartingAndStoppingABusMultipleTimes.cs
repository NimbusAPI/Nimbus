using System.Threading.Tasks;
using Nimbus.Configuration;
using Nimbus.Tests.Integration.TestScenarioGeneration.ScenarioComposition;
using Nimbus.Tests.Integration.TestScenarioGeneration.TestCaseSources;
using NUnit.Framework;

namespace Nimbus.Tests.Integration.Tests.BusStartingAndStopping
{
    [TestFixture]
    public class WhenStartingAndStoppingABusMultipleTimes : TestForBus
    {
        protected override async Task When()
        {
            await Bus.Stop();
            await Bus.Start();
            await Bus.Stop();
            await Bus.Start();
            await Bus.Stop();
        }

        [Test]
        [TestCaseSource(typeof (AllBusConfigurations<WhenStartingAndStoppingABusMultipleTimes>))]
        public async Task Run(string testName, IConfigurationScenario<BusBuilderConfiguration> scenario)
        {
            await Given(scenario);
            await When();
            await Then();
        }

        [Then]
        public async Task NothingShouldGoBang()
        {
            // if we made it here, we're good :)
        }
    }
}