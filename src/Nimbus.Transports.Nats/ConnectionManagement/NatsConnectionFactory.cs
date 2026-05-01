using NATS.Client.Core;
using Nimbus.InfrastructureContracts;

namespace Nimbus.Transports.Nats.ConnectionManagement
{
    internal class NatsConnectionFactory : IDisposable
    {
        private readonly NatsTransportConfiguration _config;
        private readonly ILogger _logger;
        private readonly Lazy<NatsConnection> _connection;

        public NatsConnectionFactory(NatsTransportConfiguration config, ILogger logger)
        {
            _config = config;
            _logger = logger;
            _connection = new Lazy<NatsConnection>(CreateConnection);
        }

        public NatsConnection GetConnection() => _connection.Value;

        public async Task TestConnection()
        {
            var opts = BuildOpts();
            await using var conn = new NatsConnection(opts);
            await conn.ConnectAsync();
            _logger.Debug("NATS connection test succeeded for {NatsUrl}", _config.NatsUrl.Value);
        }

        private NatsConnection CreateConnection()
        {
            var opts = BuildOpts();
            var conn = new NatsConnection(opts);
            conn.ConnectionOpened += (_, _) => { _logger.Debug("NATS connection opened to {NatsUrl}", _config.NatsUrl.Value); return ValueTask.CompletedTask; };
            conn.ConnectionDisconnected += (_, _) => { _logger.Debug("NATS connection disconnected"); return ValueTask.CompletedTask; };
            conn.ReconnectFailed += (_, e) => { _logger.Warn("NATS reconnect failed: {Message}", e.Message); return ValueTask.CompletedTask; };
            return conn;
        }

        private NatsOpts BuildOpts() => new NatsOpts
        {
            Url = _config.NatsUrl.Value,
            AuthOpts = _config.NatsAuthOpts,
        };

        public void Dispose()
        {
            if (_connection.IsValueCreated)
                _connection.Value.DisposeAsync().AsTask().GetAwaiter().GetResult();
        }
    }
}
