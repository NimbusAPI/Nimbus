using System;
using System.Data;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Nimbus.Configuration.Settings;
using Nimbus.Infrastructure;
using Nimbus.Infrastructure.MessageSendersAndReceivers;
using Nimbus.InfrastructureContracts;

namespace Nimbus.Transports.SqlServer.MessageSendersAndReceivers
{
    internal class SqlServerQueueReceiver : ThrottlingMessageReceiver
    {
        private readonly string _queuePath;
        private readonly SqlServerTransportConfiguration _configuration;
        private readonly ISerializer _serializer;
        private readonly ILogger _logger;

        // Atomically removes and returns the next available message for this queue.
        // UPDLOCK + READPAST allows multiple concurrent consumers without blocking each other.
        private const string FetchSql = @"
            WITH NextMessage AS (
                SELECT TOP 1 MessageId
                FROM NimbusMessages WITH (UPDLOCK, READPAST)
                WHERE Destination = @QueueName
                  AND VisibleAfter <= SYSUTCDATETIME()
                  AND (ExpiresAt IS NULL OR ExpiresAt > SYSUTCDATETIME())
                ORDER BY VisibleAfter, MessageId
            )
            DELETE m
            OUTPUT DELETED.Body
            FROM NimbusMessages m
            INNER JOIN NextMessage nm ON m.MessageId = nm.MessageId";

        public SqlServerQueueReceiver(string queuePath,
                                      SqlServerTransportConfiguration configuration,
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
                // so we don't busy-poll SQL Server.
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
            using var connection = new SqlConnection(_configuration.ConnectionString);
            await connection.OpenAsync(cancellationToken);

            using var command = new SqlCommand(FetchSql, connection);
            command.Parameters.Add("@QueueName", SqlDbType.NVarChar, 255).Value = _queuePath;

            using var reader = await command.ExecuteReaderAsync(cancellationToken);

            if (!await reader.ReadAsync(cancellationToken)) return null;

            var body = (byte[]) reader["Body"];
            var serialized = Encoding.UTF8.GetString(body);
            var message = (NimbusMessage) _serializer.Deserialize(serialized, typeof(NimbusMessage));
            _logger.Debug("Received message {MessageId} from queue {QueuePath}", message.MessageId, _queuePath);

            return message;
        }
    }
}
