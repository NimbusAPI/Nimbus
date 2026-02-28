using System.Collections.Generic;
using Nimbus.Infrastructure.Routing;
using Nimbus.InfrastructureContracts.Routing;
using Nimbus.Tests.Integration.TestScenarioGeneration.ScenarioComposition;

namespace Nimbus.Tests.Integration.TestScenarioGeneration.ConfigurationSources.Routers
{
    internal class DestinationPerMessageType : ConfigurationScenario<IRouter>
    {
        protected override IEnumerable<string> AdditionalCategories
        {
            get { yield return "SmokeTest"; }
        }

        public override ScenarioInstance<IRouter> CreateInstance()
        {
            var router = new DestinationPerMessageTypeRouter();
            var instance = new ScenarioInstance<IRouter>(router);
            return instance;
        }
    }
}