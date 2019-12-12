using System;
using Nimbus.IntegrationTests.Configuration;
using Nimbus.Configuration.Transport;
using Nimbus.Tests.Common.TestScenarioGeneration.ScenarioComposition;
using Nimbus.Transports.Redis;

namespace Nimbus.IntegrationTests.TestScenarioGeneration.ConfigurationSources.Transports
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