using System;
using System.Text;
using System.Threading.Tasks;
using Nimbus.Infrastructure.MessageSendersAndReceivers;
using Nimbus.InfrastructureContracts;
using Npgsql;
using NpgsqlTypes;

namespace Nimbus.Transports.Postgres.MessageSendersAndReceivers
{
    internal class PostgresQueueSender : INimbusMessageSender
    {
        private readonly string _queuePath;
        private readonly PostgresTransportConfiguration _configuration;
        private readonly ISerializer _serializer;

        public PostgresQueueSender(string queuePath, PostgresTransportConfiguration configuration, ISerializer serializer)
        {
            _queuePath = queuePath;
            _configuration = configuration;
            _serializer = serializer;
        }

        public async Task Send(NimbusMessage message)
        {
            var serialized = _serializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(serialized);

            var visibleAfter = message.DeliverAfter > DateTimeOffset.UtcNow
                ? message.DeliverAfter.UtcDateTime
                : DateTime.UtcNow;

            using var connection = new NpgsqlConnection(_configuration.ConnectionString);
            await connection.OpenAsync();

            const string sql = @"
                INSERT INTO nimbus_messages (message_id, destination, body, visible_after, expires_at)
                VALUES (@message_id, @destination, @body, @visible_after, @expires_at)";

            using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.Add("@message_id", NpgsqlDbType.Uuid).Value = message.MessageId;
            command.Parameters.Add("@destination", NpgsqlDbType.Text).Value = _queuePath;
            command.Parameters.Add("@body", NpgsqlDbType.Bytea).Value = body;
            command.Parameters.Add("@visible_after", NpgsqlDbType.TimestampTz).Value = visibleAfter;
            command.Parameters.Add("@expires_at", NpgsqlDbType.TimestampTz).Value = (object) message.ExpiresAfter.UtcDateTime ?? DBNull.Value;

            await command.ExecuteNonQueryAsync();
        }
    }
}
