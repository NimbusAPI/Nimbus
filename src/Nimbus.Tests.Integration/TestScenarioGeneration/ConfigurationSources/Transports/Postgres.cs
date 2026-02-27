using Nimbus.Configuration.Transport;
using Nimbus.Tests.Integration.Configuration;
using Nimbus.Tests.Integration.TestScenarioGeneration.ScenarioComposition;
using Nimbus.Transports.Postgres;

namespace Nimbus.Tests.Integration.TestScenarioGeneration.ConfigurationSources.Transports
{
    internal class Postgres : ConfigurationScenario<TransportConfiguration>
    {
        public override ScenarioInstance<TransportConfiguration> CreateInstance()
        {
            var connectionString = AppSettingsLoader.Settings.Transports.Postgres.ConnectionString;

            var configuration = new PostgresTransportConfiguration()
                .WithConnectionString(connectionString)
                .WithAutoCreateSchema();

            return new ScenarioInstance<TransportConfiguration>(configuration);
        }
    }
}
