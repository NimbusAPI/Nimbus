using System;
using System.IO;
using System.Threading.Tasks;
using Nimbus.InfrastructureContracts;
using Npgsql;

namespace Nimbus.Transports.Postgres.Schema
{
    internal class PostgresSchemaCreator
    {
        private readonly PostgresTransportConfiguration _configuration;
        private readonly ILogger _logger;

        private const string ResourceName = "Nimbus.Transports.Postgres.Schema.CreateSchema.sql";

        public PostgresSchemaCreator(PostgresTransportConfiguration configuration, ILogger logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task EnsureSchemaExists()
        {
            _logger.Debug("Ensuring Nimbus PostgreSQL schema exists...");

            var sql = ReadEmbeddedSql();

            using var connection = new NpgsqlConnection(_configuration.ConnectionString);
            await connection.OpenAsync();

            using var command = new NpgsqlCommand(sql, connection);
            await command.ExecuteNonQueryAsync();

            _logger.Debug("Nimbus PostgreSQL schema is ready.");
        }

        private static string ReadEmbeddedSql()
        {
            var assembly = typeof(PostgresSchemaCreator).Assembly;
            using var stream = assembly.GetManifestResourceStream(ResourceName)
                ?? throw new InvalidOperationException(
                    $"Embedded resource '{ResourceName}' not found in assembly '{assembly.FullName}'.");

            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
    }
}
