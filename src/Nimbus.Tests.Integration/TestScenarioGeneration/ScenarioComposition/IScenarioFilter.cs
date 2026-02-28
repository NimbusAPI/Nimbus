using System.Collections.Generic;

namespace Nimbus.Tests.Integration.TestScenarioGeneration.ScenarioComposition
{
    public interface IScenarioFilter
    {
        IEnumerable<IConfigurationScenario<T>> Filter<T>(IEnumerable<IConfigurationScenario<T>> scenarios);
    }
}