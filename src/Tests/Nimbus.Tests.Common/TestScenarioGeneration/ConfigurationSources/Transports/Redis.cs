using ConfigInjector.QuickAndDirty;
using Nimbus.Configuration.Transport;
using Nimbus.Tests.Common.Configuration;
using Nimbus.Tests.Common.TestScenarioGeneration.ScenarioComposition;
using Nimbus.Transports.Redis;

namespace Nimbus.Tests.Common.TestScenarioGeneration.ConfigurationSources.Transports
{
    internal class Redis : ConfigurationScenario<TransportConfiguration>
    {
        public override ScenarioInstance<TransportConfiguration> CreateInstance()
        {
            var connectionString = DefaultSettingsReader.Get<RedisConnectionString>();

            var configuration = new RedisTransportConfiguration()
                .WithConnectionString(connectionString);

            var instance = new ScenarioInstance<TransportConfiguration>(configuration);

            return instance;
        }
    }
}