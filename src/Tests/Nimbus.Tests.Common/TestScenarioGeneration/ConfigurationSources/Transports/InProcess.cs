using Nimbus.Configuration.Transport;
using Nimbus.Transports.InProcess;

namespace Nimbus.Tests.Common.TestScenarioGeneration.ConfigurationSources.Transports
{
    internal class InProcess : IConfigurationScenario<TransportConfiguration>
    {
        public string Name { get; } = "InProcess";
        public string[] Categories { get; } = {"InProcess", "SmokeTest"};

        public ScenarioInstance<TransportConfiguration> CreateInstance()
        {
            var configuration = new InProcessTransportConfiguration();

            var instance = new ScenarioInstance<TransportConfiguration>(configuration);

            return instance;
        }
    }
}