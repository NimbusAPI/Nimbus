using Nimbus.Configuration.Transport;
using Nimbus.Tests.Integration.Configuration;
using Nimbus.Tests.Integration.TestScenarioGeneration.ScenarioComposition;
using Nimbus.Transports.SqlServer;

namespace Nimbus.Tests.Integration.TestScenarioGeneration.ConfigurationSources.Transports
{
    internal class SqlServer : ConfigurationScenario<TransportConfiguration>
    {
        public override ScenarioInstance<TransportConfiguration> CreateInstance()
        {
            var connectionString = AppSettingsLoader.Settings.Transports.SqlServer.ConnectionString;

            var configuration = new SqlServerTransportConfiguration()
                .WithConnectionString(connectionString)
                .WithAutoCreateSchema();

            return new ScenarioInstance<TransportConfiguration>(configuration);
        }
    }
}
