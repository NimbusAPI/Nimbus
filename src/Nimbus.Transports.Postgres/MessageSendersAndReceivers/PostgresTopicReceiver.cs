using System.Threading;
using System.Threading.Tasks;
using Nimbus.Configuration.Settings;
using Nimbus.Infrastructure;
using Nimbus.InfrastructureContracts;
using Npgsql;
using NpgsqlTypes;

namespace Nimbus.Transports.Postgres.MessageSendersAndReceivers
{
    internal class PostgresTopicReceiver : PostgresQueueReceiver
    {
        private readonly PostgresSubscription _subscription;
        private readonly PostgresTransportConfiguration _configuration;

        // Idempotent subscription registration: safe to call on every bus start.
        private const string RegisterSubscriptionSql = @"
            INSERT INTO nimbus_subscriptions (topic_name, subscriber_queue)
            VALUES (@topic_name, @subscriber_queue)
            ON CONFLICT DO NOTHING";

        public PostgresTopicReceiver(PostgresSubscription subscription,
                                     PostgresTransportConfiguration configuration,
                                     ISerializer serializer,
                                     ConcurrentHandlerLimitSetting concurrentHandlerLimit,
                                     IGlobalHandlerThrottle globalHandlerThrottle,
                                     ILogger logger)
            : base(subscription.SubscriberQueueName, configuration, serializer, concurrentHandlerLimit, globalHandlerThrottle, logger)
        {
            _subscription = subscription;
            _configuration = configuration;
        }

        protected override async Task WarmUp()
        {
            using var connection = new NpgsqlConnection(_configuration.ConnectionString);
            await connection.OpenAsync();

            using var command = new NpgsqlCommand(RegisterSubscriptionSql, connection);
            command.Parameters.Add("@topic_name", NpgsqlDbType.Text).Value = _subscription.TopicPath;
            command.Parameters.Add("@subscriber_queue", NpgsqlDbType.Text).Value = _subscription.SubscriberQueueName;

            await command.ExecuteNonQueryAsync();

            await base.WarmUp();
        }

        // Override Fetch to stamp the subscriber queue name on the message so that
        // the delayed delivery service can re-route failed messages directly back to
        // this subscription queue rather than re-publishing to the topic (which would
        // fan-out to all subscribers again).
        protected override async Task<NimbusMessage> Fetch(CancellationToken cancellationToken)
        {
            var message = await base.Fetch(cancellationToken);
            if (message != null)
                message.Properties[MessagePropertyKeys.RedeliveryToSubscriptionName] = _subscription.SubscriberQueueName;
            return message;
        }
    }
}
