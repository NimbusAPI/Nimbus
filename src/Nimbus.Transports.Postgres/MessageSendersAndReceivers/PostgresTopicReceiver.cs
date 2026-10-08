using System;
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

        private readonly AutoDeleteOnIdleSetting _autoDeleteOnIdle;
        private readonly ILogger _logger;
        private DateTimeOffset _lastHeartbeat;

        // Idempotent subscription registration and heartbeat in one statement: safe to call on every bus start,
        // and re-creates the row if the reaper removed it during a long outage.
        private const string RegisterSubscriptionSql = @"
            INSERT INTO nimbus_subscriptions (topic_name, subscriber_queue, last_seen_at)
            VALUES (@topic_name, @subscriber_queue, now())
            ON CONFLICT (topic_name, subscriber_queue) DO UPDATE SET last_seen_at = now()";

        public PostgresTopicReceiver(PostgresSubscription subscription,
                                     PostgresTransportConfiguration configuration,
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
            using var connection = new NpgsqlConnection(_configuration.ConnectionString);
            await connection.OpenAsync();

            using var command = new NpgsqlCommand(RegisterSubscriptionSql, connection);
            command.Parameters.Add("@topic_name", NpgsqlDbType.Text).Value = _subscription.TopicPath;
            command.Parameters.Add("@subscriber_queue", NpgsqlDbType.Text).Value = _subscription.SubscriberQueueName;

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
