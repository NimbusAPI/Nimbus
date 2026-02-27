using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Nimbus.InfrastructureContracts;

namespace Nimbus.Transports.SqlServer.Schema
{
    internal class SqlServerSchemaCreator
    {
        private readonly SqlServerTransportConfiguration _configuration;
        private readonly ILogger _logger;

        private const string ResourceName = "Nimbus.Transports.SqlServer.Schema.CreateSchema.sql";

        public SqlServerSchemaCreator(SqlServerTransportConfiguration configuration, ILogger logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task EnsureSchemaExists()
        {
            _logger.Debug("Ensuring Nimbus SQL Server schema exists...");

            var sql = ReadEmbeddedSql();

            using var connection = new SqlConnection(_configuration.ConnectionString);
            await connection.OpenAsync();

            using var command = new SqlCommand(sql, connection);
            await command.ExecuteNonQueryAsync();

            _logger.Debug("Nimbus SQL Server schema is ready.");
        }

        private static string ReadEmbeddedSql()
        {
            var assembly = typeof(SqlServerSchemaCreator).Assembly;
            using var stream = assembly.GetManifestResourceStream(ResourceName)
                ?? throw new InvalidOperationException(
                    $"Embedded resource '{ResourceName}' not found in assembly '{assembly.FullName}'.");

            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
    }
}
