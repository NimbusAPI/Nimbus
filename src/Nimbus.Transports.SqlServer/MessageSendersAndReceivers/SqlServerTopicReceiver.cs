using System;
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

        private readonly AutoDeleteOnIdleSetting _autoDeleteOnIdle;
        private readonly ILogger _logger;
        private DateTimeOffset _lastHeartbeat;

        // Idempotent subscription registration and heartbeat in one statement: safe to call on every bus start,
        // and re-creates the row if the reaper removed it during a long outage.
        private const string RegisterSubscriptionSql = @"
            MERGE NimbusSubscriptions WITH (HOLDLOCK) AS target
            USING (SELECT @TopicName AS TopicName, @SubscriberQueue AS SubscriberQueue) AS source
               ON target.TopicName = source.TopicName AND target.SubscriberQueue = source.SubscriberQueue
            WHEN MATCHED THEN UPDATE SET LastSeenAt = SYSUTCDATETIME()
            WHEN NOT MATCHED THEN INSERT (TopicName, SubscriberQueue, LastSeenAt)
                                  VALUES (source.TopicName, source.SubscriberQueue, SYSUTCDATETIME());";

        public SqlServerTopicReceiver(SqlServerSubscription subscription,
                                      SqlServerTransportConfiguration configuration,
                                      AutoDeleteOnIdleSetting autoDeleteOnIdle,
                                      ISerializer serializer,
                                      ConcurrentHandlerLimitSetting concurrentHandlerLimit,
                                      IGlobalHandlerThrottle globalHandlerThrottle,
                                      ILogger logger)
            : base(subscription.SubscriberQueueName, configuration, serializer, concurrentHandlerLimit, globalHandlerThrottle, logger)
        {
            _subscription = subscription;
            _configuration = configuration;
            _autoDeleteOnIdle = autoDeleteOnIdle;
            _logger = logger;
        }

        protected override async Task WarmUp()
        {
            await RegisterOrTouchSubscription();
            await base.WarmUp();
        }

        private async Task RegisterOrTouchSubscription()
        {
            using var connection = new SqlConnection(_configuration.ConnectionString);
            await connection.OpenAsync();

            using var command = new SqlCommand(RegisterSubscriptionSql, connection);
            command.Parameters.Add("@TopicName", SqlDbType.NVarChar, 255).Value = _subscription.TopicPath;
            command.Parameters.Add("@SubscriberQueue", SqlDbType.NVarChar, 255).Value = _subscription.SubscriberQueueName;

            await command.ExecuteNonQueryAsync();
            _lastHeartbeat = DateTimeOffset.UtcNow;
        }

        // Refresh the heartbeat several times per idle window so one failed attempt doesn't get us reaped.
        private async Task HeartbeatIfDue()
        {
            if (DateTimeOffset.UtcNow - _lastHeartbeat < TimeSpan.FromTicks(_autoDeleteOnIdle.Value.Ticks / 4)) return;

            try
            {
                await RegisterOrTouchSubscription();
            }
            catch (Exception exc)
            {
                _logger.Warn(exc, "Failed to refresh heartbeat for subscription {SubscriberQueue}: {Message}", _subscription.SubscriberQueueName, exc.Message);
            }
        }

        // Override Fetch to stamp the subscriber queue name on the message so that
        // the delayed delivery service can re-route failed messages directly back to
        // this subscription queue rather than re-publishing to the topic (which would
        // fan-out to all subscribers again).
        protected override async Task<NimbusMessage> Fetch(CancellationToken cancellationToken)
        {
            await HeartbeatIfDue();

            var message = await base.Fetch(cancellationToken);
            if (message != null)
                message.Properties[MessagePropertyKeys.RedeliveryToSubscriptionName] = _subscription.SubscriberQueueName;
            return message;
        }
    }
}
