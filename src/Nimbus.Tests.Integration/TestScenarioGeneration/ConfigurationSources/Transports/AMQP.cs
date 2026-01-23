using Nimbus.Configuration.Transport;
using Nimbus.Tests.Integration.Configuration;
using Nimbus.Tests.Integration.TestScenarioGeneration.ScenarioComposition;
using Nimbus.Transports.AMQP;

namespace Nimbus.Tests.Integration.TestScenarioGeneration.ConfigurationSources.Transports
{
    internal class ActiveMQ : ConfigurationScenario<TransportConfiguration>
    {
        public override ScenarioInstance<TransportConfiguration> CreateInstance()
        {
            var settings = AppSettingsLoader.Settings.Transports.ActiveMQ;

            var configuration = new AMQPTransportConfiguration()
                .WithBrokerUri(settings.BrokerUri)
                .WithCredentials(settings.Username, settings.Password)
                .WithConnectionPoolSize(5);

            var instance = new ScenarioInstance<TransportConfiguration>(configuration);

            return instance;
        }
    }
}
