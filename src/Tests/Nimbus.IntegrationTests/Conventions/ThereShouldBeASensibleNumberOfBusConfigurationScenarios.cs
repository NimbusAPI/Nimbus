using System.Linq;
using System.Threading.Tasks;
using Nimbus.Tests.Common.TestScenarioGeneration.ScenarioComposition;
using Nimbus.Tests.Common.TestScenarioGeneration.ScenarioComposition.Filters;
using Nimbus.Tests.Common.TestScenarioGeneration.TestCaseSources;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.IntegrationTests.Conventions
{
    public class ConfigurationScenarioTests
    {
        [FilterTestCasesBy(typeof (InProcessScenariosFilter))]
        public class ScanForInProcessConfigurationScenarios
        {
        }

        [FilterTestCasesBy(typeof (AtLeastOneOfEachTypeOfScenarioFilter))]
        public class ScanForAllConfigurationScenarios
        {
        }

        [Test]
        public async Task ThereShouldBeASensibleNumberOfBusConfigurationScenarios()
        {
            var mandatory = new AllBusConfigurations<ScanForInProcessConfigurationScenarios>().ToArray();
            var complete = new AllBusConfigurations<ScanForAllConfigurationScenarios>().ToArray();

            mandatory.ShouldNotBeEmpty();
            complete.ShouldNotBeEmpty();
            mandatory.Length.ShouldNotBe(complete.Length);
        }
    }
}