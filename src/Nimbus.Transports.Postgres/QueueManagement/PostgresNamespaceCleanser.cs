using System.Threading.Tasks;
using Nimbus.Configuration;
using Npgsql;

namespace Nimbus.Transports.Postgres.QueueManagement
{
    internal class PostgresNamespaceCleanser : INamespaceCleanser
    {
        private readonly PostgresTransportConfiguration _configuration;

        // Subscriptions first, then messages, then dead letters.
        private const string CleanSql = @"
            DELETE FROM nimbus_subscriptions;
            DELETE FROM nimbus_messages;
            DELETE FROM nimbus_dead_letters;";

        public PostgresNamespaceCleanser(PostgresTransportConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task RemoveAllExistingNamespaceElements()
        {
            using var connection = new NpgsqlConnection(_configuration.ConnectionString);
            await connection.OpenAsync();

            using var command = new NpgsqlCommand(CleanSql, connection);
            await command.ExecuteNonQueryAsync();
        }
    }
}
