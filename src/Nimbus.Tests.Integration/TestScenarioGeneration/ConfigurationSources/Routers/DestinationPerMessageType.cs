using System.Collections.Generic;
using Nimbus.Infrastructure.Routing;
using Nimbus.Routing;
using Nimbus.Tests.Common.TestScenarioGeneration.ScenarioComposition;

namespace Nimbus.Tests.Common.TestScenarioGeneration.ConfigurationSources.Routers
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