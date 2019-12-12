using System.Linq;
using System.Threading.Tasks;
using Nimbus.Tests.Integration.TestScenarioGeneration.ScenarioComposition;
using Nimbus.Tests.Integration.TestScenarioGeneration.ScenarioComposition.Filters;
using Nimbus.Tests.Integration.TestScenarioGeneration.TestCaseSources;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.Tests.Integration.Conventions
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
            var inProcess = new AllBusConfigurations<ScanForInProcessConfigurationScenarios>().ToArray();
            var complete = new AllBusConfigurations<ScanForAllConfigurationScenarios>().ToArray();

            inProcess.ShouldNotBeEmpty();
            complete.ShouldNotBeEmpty();

            inProcess.Length.ShouldBeLessThan(complete.Length);
        }
    }
}