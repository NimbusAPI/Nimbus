using System.Linq;

namespace Nimbus.Tests.Common.TestScenarioGeneration.ConfigurationSources
{
    internal class CompositeScenario : IConfigurationScenario
    {
        public string Name { get; }
        public string[] Categories { get; }

        public CompositeScenario(params IConfigurationScenario[] scenarios)
        {
            Name = string.Join(".", scenarios.Select(s => s.Name));
            Categories = scenarios.SelectMany(s => s.Categories).ToArray();
        }
    }
}