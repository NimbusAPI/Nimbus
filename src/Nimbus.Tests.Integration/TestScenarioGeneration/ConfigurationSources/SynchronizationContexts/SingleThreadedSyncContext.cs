using Nimbus.Tests.Common.TestUtilities;
using Nimbus.Tests.Integration.TestScenarioGeneration.ScenarioComposition;

namespace Nimbus.Tests.Integration.TestScenarioGeneration.ConfigurationSources.SynchronizationContexts
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