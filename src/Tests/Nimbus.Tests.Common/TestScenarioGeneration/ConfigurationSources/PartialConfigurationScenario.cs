using System.Linq;

namespace Nimbus.Tests.Common.TestScenarioGeneration.ConfigurationSources
{
    public abstract class PartialConfigurationScenario
    {
        public string Name { get; set; }
        public string[] Categories { get; set; }

        protected PartialConfigurationScenario(string name, params string[] additionalCategories)
        {
            Name = name;
            Categories = new[] {name}.Union(additionalCategories).ToArray();
        }

        public static string Combine(params string[] names)
        {
            return string.Join(".", names);
        }
    }

    public class PartialConfigurationScenario<T> : PartialConfigurationScenario
    {
        public PartialConfigurationScenario(string name, T configuration, params string[] additionalCategories) : base(name, additionalCategories)
        {
            Configuration = configuration;
        }

        public T Configuration { get; private set; }

        public override string ToString()
        {
            return Name;
        }
    }
}