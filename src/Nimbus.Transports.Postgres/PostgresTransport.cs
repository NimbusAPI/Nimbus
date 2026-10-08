using System;
using System.Threading.Tasks;
using Nimbus.Configuration.PoorMansIocContainer;
using Nimbus.Infrastructure;
using Nimbus.Infrastructure.MessageSendersAndReceivers;
using Nimbus.InfrastructureContracts.Filtering.Conditions;
using Nimbus.Transports.Postgres.MessageSendersAndReceivers;
using Nimbus.Transports.Postgres.QueueManagement;
using Nimbus.Transports.Postgres.Schema;
using Npgsql;

namespace Nimbus.Transports.Postgres
{
    internal class PostgresTransport : INimbusTransport, IDisposable
    {
        private readonly PoorMansIoC _container;
        private readonly PostgresTransportConfiguration _configuration;
        private readonly PostgresSchemaCreator _schemaCreator;
        private readonly PostgresIdleSubscriptionReaper _idleSubscriptionReaper;

        public PostgresTransport(PoorMansIoC container, PostgresTransportConfiguration configuration, PostgresSchemaCreator schemaCreator,
                                 PostgresIdleSubscriptionReaper idleSubscriptionReaper)
        {
            _idleSubscriptionReaper = idleSubscriptionReaper;
            _container = container;
            _configuration = configuration;
            _schemaCreator = schemaCreator;
        }

        public async Task TestConnection()
        {
            using var connection = new NpgsqlConnection(_configuration.ConnectionString);
            await connection.OpenAsync();

            if (_configuration.AutoCreateSchema)
                await _schemaCreator.EnsureSchemaExists();

            _idleSubscriptionReaper.Start();
        }

        public INimbusMessageSender GetQueueSender(string queuePath)
        {
            return _container.ResolveWithOverrides<PostgresQueueSender>(queuePath);
        }

        public INimbusMessageReceiver GetQueueReceiver(string queuePath)
        {
            return _container.ResolveWithOverrides<PostgresQueueReceiver>(queuePath);
        }

        public INimbusMessageSender GetTopicSender(string topicPath)
        {
            return _container.ResolveWithOverrides<PostgresTopicSender>(topicPath);
        }

        public INimbusMessageReceiver GetTopicReceiver(string topicPath, string subscriptionName, IFilterCondition filter)
        {
            var subscription = new PostgresSubscription(topicPath, subscriptionName);
            return _container.ResolveWithOverrides<PostgresTopicReceiver>(subscription);
        }

        public void Dispose()
        {
            _idleSubscriptionReaper.Dispose();
        }
    }
}
