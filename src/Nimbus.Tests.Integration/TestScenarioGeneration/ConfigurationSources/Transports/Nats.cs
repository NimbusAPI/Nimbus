using Nimbus.Configuration.Transport;
using Nimbus.Tests.Integration.Configuration;
using Nimbus.Tests.Integration.TestScenarioGeneration.ScenarioComposition;
using Nimbus.Transports.Nats;

namespace Nimbus.Tests.Integration.TestScenarioGeneration.ConfigurationSources.Transports
{
    internal class Nats : ConfigurationScenario<TransportConfiguration>
    {
        public override ScenarioInstance<TransportConfiguration> CreateInstance()
        {
            var nats = AppSettingsLoader.Settings.Transports.Nats;

            var configuration = new NatsTransportConfiguration()
                .WithUrl(nats.Url)
                .WithCredentials(nats.Username, nats.Password);

            return new ScenarioInstance<TransportConfiguration>(configuration);
        }
    }
}
