using System;
using Nimbus.Tests.Common.TestScenarioGeneration.ScenarioComposition;
using Nimbus.Tests.Common.TestUtilities;

namespace Nimbus.Tests.Common.TestScenarioGeneration.ConfigurationSources.SynchronizationContexts
{
    public class SingleThreadedSyncContext : ConfigurationScenario<SyncContextConfiguration>
    {
        public override ScenarioInstance<SyncContextConfiguration> CreateInstance()
        {
            var syncContext = new SingleThreadedSynchronizationContext();
            var instance = new ScenarioInstance<SyncContextConfiguration>(new SyncContextConfiguration());
            instance.Disposing += (s, e) => syncContext.Dispose();
            return instance;
        }
    }
}