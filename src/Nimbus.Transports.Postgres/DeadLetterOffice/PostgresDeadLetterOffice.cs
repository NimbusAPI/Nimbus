using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nimbus.InfrastructureContracts;
using Npgsql;
using NpgsqlTypes;

namespace Nimbus.Transports.Postgres.DeadLetterOffice
{
    internal class PostgresDeadLetterOffice : IDeadLetterOffice
    {
        private readonly PostgresTransportConfiguration _configuration;
        private readonly ISerializer _serializer;

        private const string PostSql = @"
            INSERT INTO nimbus_dead_letters
                (message_id, original_destination, body, delivery_attempts, failed_at)
            VALUES
                (@message_id, @original_destination, @body, @delivery_attempts, @failed_at)";

        private const string PeekSql = @"
            SELECT body
            FROM nimbus_dead_letters
            ORDER BY failed_at ASC
            LIMIT 1";

        private const string PopSql = @"
            DELETE FROM nimbus_dead_letters
            WHERE message_id = (
                SELECT message_id FROM nimbus_dead_letters
                ORDER BY failed_at ASC LIMIT 1
                FOR UPDATE SKIP LOCKED
            )
            RETURNING body";

        private const string CountSql = @"
            SELECT COUNT(*) FROM nimbus_dead_letters";

        public PostgresDeadLetterOffice(PostgresTransportConfiguration configuration, ISerializer serializer)
        {
            _configuration = configuration;
            _serializer = serializer;
        }

        public async Task Post(NimbusMessage message)
        {
            var serialized = _serializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(serialized);
            var failedAt = message.DeliveryAttempts.Any()
                ? message.DeliveryAttempts.Min().UtcDateTime
                : DateTime.UtcNow;

            using var connection = new NpgsqlConnection(_configuration.ConnectionString);
            await connection.OpenAsync();

            using var command = new NpgsqlCommand(PostSql, connection);
            command.Parameters.Add("@message_id", NpgsqlDbType.Uuid).Value = message.MessageId;
            command.Parameters.Add("@original_destination", NpgsqlDbType.Text).Value = (object) message.DeliverTo ?? DBNull.Value;
            command.Parameters.Add("@body", NpgsqlDbType.Bytea).Value = body;
            command.Parameters.Add("@delivery_attempts", NpgsqlDbType.Integer).Value = message.DeliveryAttempts.Length;
            command.Parameters.Add("@failed_at", NpgsqlDbType.TimestampTz).Value = failedAt;

            await command.ExecuteNonQueryAsync();
        }

        public async Task<NimbusMessage> Peek()
        {
            using var connection = new NpgsqlConnection(_configuration.ConnectionString);
            await connection.OpenAsync();

            using var command = new NpgsqlCommand(PeekSql, connection);
            using var reader = await command.ExecuteReaderAsync();

            if (!await reader.ReadAsync()) return null;

            return DeserializeBody(reader);
        }

        public async Task<NimbusMessage> Pop()
        {
            using var connection = new NpgsqlConnection(_configuration.ConnectionString);
            await connection.OpenAsync();

            using var command = new NpgsqlCommand(PopSql, connection);
            using var reader = await command.ExecuteReaderAsync();

            if (!await reader.ReadAsync()) return null;

            return DeserializeBody(reader);
        }

        public async Task<int> Count()
        {
            using var connection = new NpgsqlConnection(_configuration.ConnectionString);
            await connection.OpenAsync();

            using var command = new NpgsqlCommand(CountSql, connection);
            return (int)(long) await command.ExecuteScalarAsync();
        }

        private NimbusMessage DeserializeBody(NpgsqlDataReader reader)
        {
            var body = (byte[]) reader["body"];
            var serialized = Encoding.UTF8.GetString(body);
            return (NimbusMessage) _serializer.Deserialize(serialized, typeof(NimbusMessage));
        }
    }
}
