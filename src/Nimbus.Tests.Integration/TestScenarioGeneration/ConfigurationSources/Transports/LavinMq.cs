using Nimbus.Configuration.Transport;
using Nimbus.Tests.Integration.Configuration;
using Nimbus.Tests.Integration.TestScenarioGeneration.ScenarioComposition;
using Nimbus.Transports.RabbitMQ;

namespace Nimbus.Tests.Integration.TestScenarioGeneration.ConfigurationSources.Transports
{
    internal class LavinMq : ConfigurationScenario<TransportConfiguration>
    {
        public override ScenarioInstance<TransportConfiguration> CreateInstance()
        {
            var settings = AppSettingsLoader.Settings.Transports.LavinMq;

            var configuration = new RabbitMqTransportConfiguration()
                .WithHost(settings.Host)
                .WithPort(settings.Port)
                .WithManagementPort(settings.ManagementPort)
                .WithCredentials(settings.Username, settings.Password);

            return new ScenarioInstance<TransportConfiguration>(configuration);
        }
    }
}
