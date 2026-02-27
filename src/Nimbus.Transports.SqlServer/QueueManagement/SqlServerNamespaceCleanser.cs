using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Nimbus.Configuration;

namespace Nimbus.Transports.SqlServer.QueueManagement
{
    internal class SqlServerNamespaceCleanser : INamespaceCleanser
    {
        private readonly SqlServerTransportConfiguration _configuration;

        // Subscriptions first to avoid any FK considerations, then messages, then dead letters.
        private const string CleanSql = @"
            DELETE FROM NimbusSubscriptions;
            DELETE FROM NimbusMessages;
            DELETE FROM NimbusDeadLetters;";

        public SqlServerNamespaceCleanser(SqlServerTransportConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task RemoveAllExistingNamespaceElements()
        {
            using var connection = new SqlConnection(_configuration.ConnectionString);
            await connection.OpenAsync();

            using var command = new SqlCommand(CleanSql, connection);
            await command.ExecuteNonQueryAsync();
        }
    }
}
