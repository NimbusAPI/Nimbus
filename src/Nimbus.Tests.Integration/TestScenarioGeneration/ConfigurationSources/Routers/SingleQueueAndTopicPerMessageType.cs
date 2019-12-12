using Nimbus.Infrastructure.Routing;
using Nimbus.InfrastructureContracts.Routing;
using Nimbus.Tests.Integration.TestScenarioGeneration.ScenarioComposition;

namespace Nimbus.Tests.Integration.TestScenarioGeneration.ConfigurationSources.Routers
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