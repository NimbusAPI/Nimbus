using Nimbus.Infrastructure.Routing;
using Nimbus.Routing;
using Nimbus.Tests.Common.TestScenarioGeneration.ScenarioComposition;

namespace Nimbus.Tests.Common.TestScenarioGeneration.ConfigurationSources.Routers
{
    internal class SingleQueueAndTopicPerMessageType : ConfigurationScenario<IRouter>
    {
        public override ScenarioInstance<IRouter> CreateInstance()
        {
            var router = new SingleQueueAndTopicPerMessageTypeRouter(new DestinationPerMessageTypeRouter());
            var instance = new ScenarioInstance<IRouter>(router);
            return instance;
        }
    }
}