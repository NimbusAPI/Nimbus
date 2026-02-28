using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Nimbus.Configuration.Settings;
using Nimbus.Infrastructure;
using Nimbus.Infrastructure.MessageSendersAndReceivers;
using Nimbus.InfrastructureContracts;
using Npgsql;
using NpgsqlTypes;

namespace Nimbus.Transports.Postgres.MessageSendersAndReceivers
{
    internal class PostgresQueueReceiver : ThrottlingMessageReceiver
    {
        private readonly string _queuePath;
        private readonly PostgresTransportConfiguration _configuration;
        private readonly ISerializer _serializer;
        private readonly ILogger _logger;

        // Atomically removes and returns the next available message for this queue.
        // FOR UPDATE SKIP LOCKED allows multiple concurrent consumers without blocking each other.
        private const string FetchSql = @"
            DELETE FROM nimbus_messages
            WHERE message_id = (
                SELECT message_id
                FROM   nimbus_messages
                WHERE  destination   = @queue_name
                  AND  visible_after <= now()
                  AND  (expires_at IS NULL OR expires_at > now())
                ORDER BY visible_after, message_id
                LIMIT 1
                FOR UPDATE SKIP LOCKED
            )
            RETURNING body";

        public PostgresQueueReceiver(string queuePath,
                                     PostgresTransportConfiguration configuration,
                                     ISerializer serializer,
                                     ConcurrentHandlerLimitSetting concurrentHandlerLimit,
                                     IGlobalHandlerThrottle globalHandlerThrottle,
                                     ILogger logger)
            : base(concurrentHandlerLimit, globalHandlerThrottle, logger)
        {
            _queuePath = queuePath;
            _configuration = configuration;
            _serializer = serializer;
            _logger = logger;
        }

        protected override Task WarmUp()
        {
            return Task.CompletedTask;
        }

        protected override async Task<NimbusMessage> Fetch(CancellationToken cancellationToken)
        {
            try
            {
                var message = await TryPopMessage(cancellationToken);

                if (message != null) return message;

                // Queue is empty; wait before signalling the worker loop to try again,
                // so we don't busy-poll PostgreSQL.
                await Task.Delay(_configuration.PollInterval, cancellationToken);

                return null;
            }
            catch (OperationCanceledException)
            {
                return null;
            }
        }

        private async Task<NimbusMessage> TryPopMessage(CancellationToken cancellationToken)
        {
            using var connection = new NpgsqlConnection(_configuration.ConnectionString);
            await connection.OpenAsync(cancellationToken);

            using var command = new NpgsqlCommand(FetchSql, connection);
            command.Parameters.Add("@queue_name", NpgsqlDbType.Text).Value = _queuePath;

            using var reader = await command.ExecuteReaderAsync(cancellationToken);

            if (!await reader.ReadAsync(cancellationToken)) return null;

            var body = (byte[]) reader["body"];
            var serialized = Encoding.UTF8.GetString(body);
            var message = (NimbusMessage) _serializer.Deserialize(serialized, typeof(NimbusMessage));
            _logger.Debug("Received message {MessageId} from queue {QueuePath}", message.MessageId, _queuePath);

            return message;
        }
    }
}
