using System.Collections.Generic;
using System.Threading;
using Nimbus.Tests.Integration.TestScenarioGeneration.ScenarioComposition;

namespace Nimbus.Tests.Integration.TestScenarioGeneration.ConfigurationSources.SynchronizationContexts
{
    public class NullSyncContext : ConfigurationScenario<SyncContextConfiguration>
    {
        protected override IEnumerable<string> AdditionalCategories
        {
            get { yield return "SmokeTest"; }
        }

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