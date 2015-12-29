using Nimbus.Infrastructure.Routing;
using Nimbus.Routing;

namespace Nimbus.Tests.Common.TestScenarioGeneration.ConfigurationSources.Routers
{
    internal class DestinationPerMessageType : IConfigurationScenario<IRouter>
    {
        public string Name { get; } = "DestinationPerMessageType";
        public string[] Categories { get; } = {"DestinationPerMessageType"};

        public ScenarioInstance<IRouter> CreateInstance()
        {
            var router = new DestinationPerMessageTypeRouter();
            var instance = new ScenarioInstance<IRouter>(router);
            return instance;
        }
    }
}