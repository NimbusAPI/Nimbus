using System;

namespace Nimbus.Tests.Integration.TestScenarioGeneration.ScenarioComposition
{
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