using System.Collections.Generic;
using System.Linq;

namespace Nimbus.Tests.Integration.TestScenarioGeneration.ScenarioComposition
{
    public interface IConfigurationScenario
    {
        string Name { get; }
        IEnumerable<string> Categories { get; }
        IEnumerable<IConfigurationScenario> ComposedOf { get; }
    }

    public interface IConfigurationScenario<T> : IConfigurationScenario
    {
        ScenarioInstance<T> CreateInstance();
    }

    public abstract class ConfigurationScenario : IConfigurationScenario
    {
        public string Name => GetType().Name;
        public IEnumerable<string> Categories => new[] {Name}.Union(AdditionalCategories).ToArray();
        protected virtual IEnumerable<string> AdditionalCategories => Enumerable.Empty<string>();

        public IEnumerable<IConfigurationScenario> ComposedOf
        {
            get { yield return this; }
        }
    }

    public abstract class ConfigurationScenario<T> : ConfigurationScenario, IConfigurationScenario<T>
    {
        public abstract ScenarioInstance<T> CreateInstance();
    }
}