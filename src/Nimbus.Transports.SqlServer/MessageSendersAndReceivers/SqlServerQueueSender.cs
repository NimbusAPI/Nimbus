using System;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Nimbus.Infrastructure.MessageSendersAndReceivers;
using Nimbus.InfrastructureContracts;

namespace Nimbus.Transports.SqlServer.MessageSendersAndReceivers
{
    internal class SqlServerQueueSender : INimbusMessageSender
    {
        private readonly string _queuePath;
        private readonly SqlServerTransportConfiguration _configuration;
        private readonly ISerializer _serializer;

        public SqlServerQueueSender(string queuePath, SqlServerTransportConfiguration configuration, ISerializer serializer)
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

            using var connection = new SqlConnection(_configuration.ConnectionString);
            await connection.OpenAsync();

            const string sql = @"
                INSERT INTO NimbusMessages (MessageId, Destination, Body, VisibleAfter, ExpiresAt)
                VALUES (@MessageId, @Destination, @Body, @VisibleAfter, @ExpiresAt)";

            using var command = new SqlCommand(sql, connection);
            command.Parameters.Add("@MessageId", SqlDbType.UniqueIdentifier).Value = message.MessageId;
            command.Parameters.Add("@Destination", SqlDbType.NVarChar, 255).Value = _queuePath;
            command.Parameters.Add("@Body", SqlDbType.VarBinary, -1).Value = body;
            command.Parameters.Add("@VisibleAfter", SqlDbType.DateTime2).Value = visibleAfter;
            command.Parameters.Add("@ExpiresAt", SqlDbType.DateTime2).Value = (object) message.ExpiresAfter.UtcDateTime ?? DBNull.Value;

            await command.ExecuteNonQueryAsync();
        }
    }
}
