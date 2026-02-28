using System.Collections;
using System.Collections.Generic;
using Nimbus.Tests.Integration.TestScenarioGeneration.ScenarioComposition;

namespace Nimbus.Tests.Integration.TestScenarioGeneration.ConfigurationSources.SynchronizationContexts
{
    public class SynchronizationContextConfigurationSources : IEnumerable<IConfigurationScenario<SyncContextConfiguration>>
    {
        public IEnumerator<IConfigurationScenario<SyncContextConfiguration>> GetEnumerator()
        {
            yield return new NullSyncContext();
            yield return new SingleThreadedSyncContext();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}