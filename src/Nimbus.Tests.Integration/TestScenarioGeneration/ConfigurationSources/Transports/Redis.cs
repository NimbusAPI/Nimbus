using Nimbus.Configuration.Transport;
using Nimbus.Tests.Integration.Configuration;
using Nimbus.Tests.Integration.TestScenarioGeneration.ScenarioComposition;
using Nimbus.Transports.Redis;

namespace Nimbus.Tests.Integration.TestScenarioGeneration.ConfigurationSources.Transports
{
    internal class Redis : ConfigurationScenario<TransportConfiguration>
    {
        public override ScenarioInstance<TransportConfiguration> CreateInstance()
        {
            var connectionString =  AppSettingsLoader.Settings.Transports.Redis.ConnectionString;

            var configuration = new RedisTransportConfiguration()
                .WithConnectionString(connectionString);

            var instance = new ScenarioInstance<TransportConfiguration>(configuration);

            return instance;
        }
    }
}