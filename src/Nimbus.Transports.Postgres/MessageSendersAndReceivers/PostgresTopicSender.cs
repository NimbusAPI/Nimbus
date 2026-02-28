using System;
using System.Text;
using System.Threading.Tasks;
using Nimbus.Infrastructure.MessageSendersAndReceivers;
using Nimbus.InfrastructureContracts;
using Npgsql;
using NpgsqlTypes;

namespace Nimbus.Transports.Postgres.MessageSendersAndReceivers
{
    internal class PostgresTopicSender : INimbusMessageSender
    {
        private readonly string _topicPath;
        private readonly PostgresTransportConfiguration _configuration;
        private readonly ISerializer _serializer;

        // Fan-out: one INSERT per subscriber, each with a fresh UUID, all in one atomic statement.
        // gen_random_uuid() is evaluated per row, so every subscriber gets a unique message_id.
        private const string FanOutSql = @"
            INSERT INTO nimbus_messages (message_id, destination, body, visible_after, expires_at)
            SELECT gen_random_uuid(), subscriber_queue, @body, @visible_after, @expires_at
            FROM   nimbus_subscriptions
            WHERE  topic_name = @topic_name";

        public PostgresTopicSender(string topicPath, PostgresTransportConfiguration configuration, ISerializer serializer)
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

            using var connection = new NpgsqlConnection(_configuration.ConnectionString);
            await connection.OpenAsync();

            using var command = new NpgsqlCommand(FanOutSql, connection);
            command.Parameters.Add("@topic_name", NpgsqlDbType.Text).Value = _topicPath;
            command.Parameters.Add("@body", NpgsqlDbType.Bytea).Value = body;
            command.Parameters.Add("@visible_after", NpgsqlDbType.TimestampTz).Value = visibleAfter;
            command.Parameters.Add("@expires_at", NpgsqlDbType.TimestampTz).Value = (object) message.ExpiresAfter.UtcDateTime ?? DBNull.Value;

            await command.ExecuteNonQueryAsync();
        }
    }
}
