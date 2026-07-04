using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nimbus.InfrastructureContracts;
using RabbitMQ.Client;

namespace Nimbus.Transports.RabbitMQ.ConnectionManagement
{
    internal class RabbitMqConnectionManager : IAsyncDisposable
    {
        private readonly RabbitMqTransportConfiguration _configuration;
        private readonly ILogger _logger;
        private readonly SemaphoreSlim _connectionLock = new(1, 1);
        private IConnection _connection;
        private bool _disposed;

        internal const string DelayedExchangeName = "nimbus.delayed";

        public RabbitMqConnectionManager(RabbitMqTransportConfiguration configuration, ILogger logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<IChannel> CreateChannelAsync()
        {
            var connection = await GetOrCreateConnectionAsync();
            return await connection.CreateChannelAsync();
        }

        public async Task TestConnectionAsync()
        {
            var channel = await CreateChannelAsync();
            try { await channel.CloseAsync(); } catch { }
            channel.Dispose();
            _logger.Info("RabbitMQ connection test successful");
        }

        public async Task EnsureDelayedExchangeDeclaredAsync(IChannel channel)
        {
            var args = new Dictionary<string, object> { { "x-delayed-type", "direct" } };
            await channel.ExchangeDeclareAsync(
                exchange: DelayedExchangeName,
                type: "x-delayed-message",
                durable: true,
                autoDelete: false,
                arguments: args);

            _logger.Debug("Declared delayed exchange '{ExchangeName}'", DelayedExchangeName);
        }

        private async Task<IConnection> GetOrCreateConnectionAsync()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(RabbitMqConnectionManager));
            if (_connection != null) return _connection;

            await _connectionLock.WaitAsync();
            try
            {
                if (_connection != null) return _connection;

                var factory = new ConnectionFactory
                {
                    HostName = _configuration.Host,
                    Port = _configuration.Port,
                    UserName = _configuration.Username,
                    Password = _configuration.Password,
                    VirtualHost = _configuration.VirtualHost,
                    // Default is 1, which means a single blocked consumer callback stalls all
                    // consumers on the connection. A small pool prevents that.
                    ConsumerDispatchConcurrency = (ushort)Math.Max(2, Environment.ProcessorCount),
                };
                
                _connection = await factory.CreateConnectionAsync();
                _logger.Info("Created RabbitMQ connection to {Host}:{Port}", _configuration.Host, _configuration.Port);
                return _connection;
            }
            finally
            {
                _connectionLock.Release();
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_disposed) return;
            _disposed = true;

            try
            {
                if (_connection != null)
                {
                    await _connection.CloseAsync();
                    _connection.Dispose();
                }
            }
            catch (Exception ex)
            {
                _logger.Warn(ex, "Error disposing RabbitMQ connection");
            }

            _connectionLock.Dispose();
        }
    }
}
