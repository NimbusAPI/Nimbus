using System.Linq;
using System.Threading.Tasks;
using Nimbus.Tests.Common.TestScenarioGeneration.TestCaseSources;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.IntegrationTests.Conventions
{
    public class ConfigurationScenarioTests
    {
        [Test]
        public async Task ThereShouldBeASensibleNumberOfBusConfigurationScenarios()
        {
            var sources = new AllBusConfigurations<ConfigurationScenarioTests>().ToArray();
            sources.ShouldNotBeEmpty();
        }
    }
}