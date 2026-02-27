using System;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Nimbus.Infrastructure.MessageSendersAndReceivers;
using Nimbus.InfrastructureContracts;

namespace Nimbus.Transports.SqlServer.MessageSendersAndReceivers
{
    internal class SqlServerTopicSender : INimbusMessageSender
    {
        private readonly string _topicPath;
        private readonly SqlServerTransportConfiguration _configuration;
        private readonly ISerializer _serializer;

        // Fan-out: one INSERT per subscriber, each with a fresh GUID, all in one atomic statement.
        // NEWID() is evaluated per row, so every subscriber gets a unique MessageId.
        // Filters in NimbusSubscriptions are not evaluated yet (Phase 1 delivers to all subscribers).
        private const string FanOutSql = @"
            INSERT INTO NimbusMessages (MessageId, Destination, Body, VisibleAfter, ExpiresAt)
            SELECT NEWID(), SubscriberQueue, @Body, @VisibleAfter, @ExpiresAt
            FROM NimbusSubscriptions
            WHERE TopicName = @TopicName";

        public SqlServerTopicSender(string topicPath, SqlServerTransportConfiguration configuration, ISerializer serializer)
        {
            _topicPath = topicPath;
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

            using var command = new SqlCommand(FanOutSql, connection);
            command.Parameters.Add("@TopicName", SqlDbType.NVarChar, 255).Value = _topicPath;
            command.Parameters.Add("@Body", SqlDbType.VarBinary, -1).Value = body;
            command.Parameters.Add("@VisibleAfter", SqlDbType.DateTime2).Value = visibleAfter;
            command.Parameters.Add("@ExpiresAt", SqlDbType.DateTime2).Value = (object) message.ExpiresAfter.UtcDateTime ?? DBNull.Value;

            await command.ExecuteNonQueryAsync();
        }
    }
}
