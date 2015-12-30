using System.Linq;

namespace Nimbus.Tests.Common.TestScenarioGeneration.ConfigurationSources
{
    internal class CompositeScenario : IConfigurationScenario
    {
        public string Name { get; }
        public string[] Categories { get; }

        private static readonly string[] _andCategories = {"SmokeTest"};

        public CompositeScenario(params IConfigurationScenario[] scenarios)
        {
            Name = string.Join(".", scenarios.Select(s => s.Name));

            var normalCategories = scenarios
                .SelectMany(s => s.Categories)
                .Except(_andCategories)
                .ToArray();

            var additionalCategories = _andCategories
                .Where(cat => scenarios.All(s => s.Categories.Contains(cat)))
                .ToArray();

            Categories = new string[0]
                .Union(normalCategories)
                .Union(additionalCategories)
                .ToArray();
        }
    }
}