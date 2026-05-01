using Nimbus.Configuration.Transport;
using Nimbus.Tests.Integration.Configuration;
using Nimbus.Tests.Integration.TestScenarioGeneration.ScenarioComposition;
using Nimbus.Transports.Nats;

namespace Nimbus.Tests.Integration.TestScenarioGeneration.ConfigurationSources.Transports;

internal class NatsJetStream : ConfigurationScenario<TransportConfiguration>
{
    public override ScenarioInstance<TransportConfiguration> CreateInstance()
    {
        var url = AppSettingsLoader.Settings.Transports.Nats.Url;

        var configuration = new NatsTransportConfiguration()
            .WithJetStream()
            .WithUrl(url);

        return new ScenarioInstance<TransportConfiguration>(configuration);
    }
}