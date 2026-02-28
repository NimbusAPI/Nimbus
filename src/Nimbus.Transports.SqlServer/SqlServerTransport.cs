using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Nimbus.Configuration.PoorMansIocContainer;
using Nimbus.Infrastructure;
using Nimbus.Infrastructure.MessageSendersAndReceivers;
using Nimbus.InfrastructureContracts.Filtering.Conditions;
using Nimbus.Transports.SqlServer.MessageSendersAndReceivers;
using Nimbus.Transports.SqlServer.Schema;

namespace Nimbus.Transports.SqlServer
{
    internal class SqlServerTransport : INimbusTransport
    {
        private readonly PoorMansIoC _container;
        private readonly SqlServerTransportConfiguration _configuration;
        private readonly SqlServerSchemaCreator _schemaCreator;

        public SqlServerTransport(PoorMansIoC container, SqlServerTransportConfiguration configuration, SqlServerSchemaCreator schemaCreator)
        {
            _container = container;
            _configuration = configuration;
            _schemaCreator = schemaCreator;
        }

        public async Task TestConnection()
        {
            using var connection = new SqlConnection(_configuration.ConnectionString);
            await connection.OpenAsync();

            if (_configuration.AutoCreateSchema)
                await _schemaCreator.EnsureSchemaExists();
        }

        public INimbusMessageSender GetQueueSender(string queuePath)
        {
            return _container.ResolveWithOverrides<SqlServerQueueSender>(queuePath);
        }

        public INimbusMessageReceiver GetQueueReceiver(string queuePath)
        {
            return _container.ResolveWithOverrides<SqlServerQueueReceiver>(queuePath);
        }

        public INimbusMessageSender GetTopicSender(string topicPath)
        {
            return _container.ResolveWithOverrides<SqlServerTopicSender>(topicPath);
        }

        public INimbusMessageReceiver GetTopicReceiver(string topicPath, string subscriptionName, IFilterCondition filter)
        {
            var subscription = new SqlServerSubscription(topicPath, subscriptionName);
            return _container.ResolveWithOverrides<SqlServerTopicReceiver>(subscription);
        }
    }
}
