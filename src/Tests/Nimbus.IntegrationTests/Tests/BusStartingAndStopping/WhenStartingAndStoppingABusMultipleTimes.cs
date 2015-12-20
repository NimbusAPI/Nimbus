using System.Threading.Tasks;
using Nimbus.Configuration;
using Nimbus.IntegrationTests.TestScenarioGeneration;
using NUnit.Framework;

namespace Nimbus.IntegrationTests.Tests.BusStartingAndStopping
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
        public async Task NothingShouldGoBang(string testName, BusBuilderConfiguration busBuilderConfiguration)
        {
            await Given(busBuilderConfiguration);
            await When();
        }
    }
}