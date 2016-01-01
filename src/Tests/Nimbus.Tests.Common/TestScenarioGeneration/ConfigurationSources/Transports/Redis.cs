using ConfigInjector.QuickAndDirty;
using Nimbus.Configuration.Transport;
using Nimbus.Tests.Common.Configuration;
using Nimbus.Transports.Redis;

namespace Nimbus.Tests.Common.TestScenarioGeneration.ConfigurationSources.Transports
{
    internal class Redis : IConfigurationScenario<TransportConfiguration>
    {
        public string Name { get; } = "Redis";
        public string[] Categories { get; } = {"Redis"};

        public ScenarioInstance<TransportConfiguration> CreateInstance()
        {
            var connectionString = DefaultSettingsReader.Get<RedisConnectionString>();

            var configuration = new RedisTransportConfiguration()
                .WithConnectionString(connectionString);

            var instance = new ScenarioInstance<TransportConfiguration>(configuration);

            return instance;
        }
    }
}