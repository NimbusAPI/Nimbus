using System;
using System.Threading;
using Nimbus.Tests.Common.TestScenarioGeneration.ScenarioComposition;

namespace Nimbus.Tests.Common.TestScenarioGeneration.ConfigurationSources.SynchronizationContexts
{
    public class NullSyncContext : ConfigurationScenario<SyncContextConfiguration>
    {
        public override ScenarioInstance<SyncContextConfiguration> CreateInstance()
        {
            var instance = new ScenarioInstance<SyncContextConfiguration>(new SyncContextConfiguration());
            var previousSyncContext = SynchronizationContext.Current;
            SynchronizationContext.SetSynchronizationContext(null);
            instance.Disposing += (s, e) => SynchronizationContext.SetSynchronizationContext(previousSyncContext);
            return instance;
        }
    }
}