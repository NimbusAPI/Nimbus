using NATS.Client.Core;
using Nimbus.InfrastructureContracts;
using Nimbus.Transports.Nats.Configuration;

namespace Nimbus.Transports.Nats.ConnectionManagement
{
    internal class NatsConnectionFactory : IDisposable
    {
        private readonly NatsUrl _natsUrl;
        private readonly ILogger _logger;
        private readonly Lazy<NatsConnection> _connection;

        public NatsConnectionFactory(NatsUrl natsUrl, ILogger logger)
        {
            _natsUrl = natsUrl;
            _logger = logger;
            _connection = new Lazy<NatsConnection>(CreateConnection);
        }

        public NatsConnection GetConnection() => _connection.Value;

        public async Task TestConnection()
        {
            var opts = new NatsOpts { Url = _natsUrl.Value };
            await using var conn = new NatsConnection(opts);
            await conn.ConnectAsync();
            _logger.Debug("NATS connection test succeeded for {NatsUrl}", _natsUrl.Value);
        }

        private NatsConnection CreateConnection()
        {
            var opts = new NatsOpts { Url = _natsUrl.Value };
            var conn = new NatsConnection(opts);
            conn.ConnectionOpened += (_, _) => { _logger.Debug("NATS connection opened to {NatsUrl}", _natsUrl.Value); return ValueTask.CompletedTask; };
            conn.ConnectionDisconnected += (_, _) => { _logger.Debug("NATS connection disconnected"); return ValueTask.CompletedTask; };
            conn.ReconnectFailed += (_, e) => { _logger.Warn("NATS reconnect failed: {Message}", e.Message); return ValueTask.CompletedTask; };
            return conn;
        }

        public void Dispose()
        {
            if (_connection.IsValueCreated)
                _connection.Value.DisposeAsync().AsTask().GetAwaiter().GetResult();
        }
    }
}
