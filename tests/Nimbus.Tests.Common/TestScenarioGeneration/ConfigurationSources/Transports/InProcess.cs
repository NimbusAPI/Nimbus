using System.Collections.Generic;
using Nimbus.Configuration.Transport;
using Nimbus.Tests.Common.TestScenarioGeneration.ScenarioComposition;
using Nimbus.Transports.InProcess;

namespace Nimbus.Tests.Common.TestScenarioGeneration.ConfigurationSources.Transports
{
    internal class InProcess : ConfigurationScenario<TransportConfiguration>
    {
        protected override IEnumerable<string> AdditionalCategories
        {
            get { yield return "SmokeTest"; }
        }

        public override ScenarioInstance<TransportConfiguration> CreateInstance()
        {
            var configuration = new InProcessTransportConfiguration();

            var instance = new ScenarioInstance<TransportConfiguration>(configuration);

            return instance;
        }
    }
}