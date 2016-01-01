using System;

namespace Nimbus.Tests.Common.TestScenarioGeneration.ConfigurationSources
{
    public interface IConfigurationScenario
    {
        string Name { get; }
        string[] Categories { get; }
    }

    public interface IConfigurationScenario<T> : IConfigurationScenario
    {
        ScenarioInstance<T> CreateInstance();
    }

    public class ScenarioInstance<T> : IDisposable
    {
        public EventHandler<EventArgs> Disposing;

        public T Configuration { get; }

        public ScenarioInstance(T configuration)
        {
            Configuration = configuration;
        }

        public void Dispose()
        {
            Disposing?.Invoke(this, EventArgs.Empty);
        }
    }
}