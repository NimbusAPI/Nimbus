using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Nimbus.Configuration.Settings;
using Nimbus.Infrastructure;
using Nimbus.InfrastructureContracts;

namespace Nimbus.Transports.SqlServer.MessageSendersAndReceivers
{
    internal class SqlServerTopicReceiver : SqlServerQueueReceiver
    {
        private readonly SqlServerSubscription _subscription;
        private readonly SqlServerTransportConfiguration _configuration;

        // Idempotent subscription registration: safe to call on every bus start.
        private const string RegisterSubscriptionSql = @"
            IF NOT EXISTS (
                SELECT 1 FROM NimbusSubscriptions
                WHERE TopicName = @TopicName AND SubscriberQueue = @SubscriberQueue
            )
            INSERT INTO NimbusSubscriptions (TopicName, SubscriberQueue)
            VALUES (@TopicName, @SubscriberQueue)";

        public SqlServerTopicReceiver(SqlServerSubscription subscription,
                                      SqlServerTransportConfiguration configuration,
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
            using var connection = new SqlConnection(_configuration.ConnectionString);
            await connection.OpenAsync();

            using var command = new SqlCommand(RegisterSubscriptionSql, connection);
            command.Parameters.Add("@TopicName", SqlDbType.NVarChar, 255).Value = _subscription.TopicPath;
            command.Parameters.Add("@SubscriberQueue", SqlDbType.NVarChar, 255).Value = _subscription.SubscriberQueueName;

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
