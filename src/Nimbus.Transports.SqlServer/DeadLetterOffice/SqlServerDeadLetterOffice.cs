using System;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Nimbus.InfrastructureContracts;

namespace Nimbus.Transports.SqlServer.DeadLetterOffice
{
    internal class SqlServerDeadLetterOffice : IDeadLetterOffice
    {
        private readonly SqlServerTransportConfiguration _configuration;
        private readonly ISerializer _serializer;

        private const string PostSql = @"
            INSERT INTO NimbusDeadLetters
                (MessageId, OriginalDestination, Body, DeliveryAttempts, FailedAt)
            VALUES
                (@MessageId, @OriginalDestination, @Body, @DeliveryAttempts, @FailedAt)";

        private const string PeekSql = @"
            SELECT TOP 1 Body
            FROM NimbusDeadLetters
            ORDER BY FailedAt ASC";

        private const string PopSql = @"
            WITH Oldest AS (
                SELECT TOP 1 MessageId
                FROM NimbusDeadLetters
                ORDER BY FailedAt ASC
            )
            DELETE d
            OUTPUT DELETED.Body
            FROM NimbusDeadLetters d
            INNER JOIN Oldest o ON d.MessageId = o.MessageId";

        private const string CountSql = @"
            SELECT COUNT(*) FROM NimbusDeadLetters";

        public SqlServerDeadLetterOffice(SqlServerTransportConfiguration configuration, ISerializer serializer)
        {
            _configuration = configuration;
            _serializer = serializer;
        }

        public async Task Post(NimbusMessage message)
        {
            var serialized = _serializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(serialized);
            var enqueuedAt = message.DeliveryAttempts.Any()
                ? message.DeliveryAttempts.Min().UtcDateTime
                : DateTime.UtcNow;

            using var connection = new SqlConnection(_configuration.ConnectionString);
            await connection.OpenAsync();

            using var command = new SqlCommand(PostSql, connection);
            command.Parameters.Add("@MessageId", SqlDbType.UniqueIdentifier).Value = message.MessageId;
            command.Parameters.Add("@OriginalDestination", SqlDbType.NVarChar, 255).Value = message.DeliverTo ?? (object) DBNull.Value;
            command.Parameters.Add("@Body", SqlDbType.VarBinary, -1).Value = body;
            command.Parameters.Add("@DeliveryAttempts", SqlDbType.Int).Value = message.DeliveryAttempts.Length;
            command.Parameters.Add("@FailedAt", SqlDbType.DateTime2).Value = enqueuedAt;

            await command.ExecuteNonQueryAsync();
        }

        public async Task<NimbusMessage> Peek()
        {
            using var connection = new SqlConnection(_configuration.ConnectionString);
            await connection.OpenAsync();

            using var command = new SqlCommand(PeekSql, connection);
            using var reader = await command.ExecuteReaderAsync();

            if (!await reader.ReadAsync()) return null;

            return DeserializeBody(reader);
        }

        public async Task<NimbusMessage> Pop()
        {
            using var connection = new SqlConnection(_configuration.ConnectionString);
            await connection.OpenAsync();

            using var command = new SqlCommand(PopSql, connection);
            using var reader = await command.ExecuteReaderAsync();

            if (!await reader.ReadAsync()) return null;

            return DeserializeBody(reader);
        }

        public async Task<int> Count()
        {
            using var connection = new SqlConnection(_configuration.ConnectionString);
            await connection.OpenAsync();

            using var command = new SqlCommand(CountSql, connection);
            return (int) await command.ExecuteScalarAsync();
        }

        private NimbusMessage DeserializeBody(SqlDataReader reader)
        {
            var body = (byte[]) reader["Body"];
            var serialized = Encoding.UTF8.GetString(body);
            return (NimbusMessage) _serializer.Deserialize(serialized, typeof(NimbusMessage));
        }
    }
}
