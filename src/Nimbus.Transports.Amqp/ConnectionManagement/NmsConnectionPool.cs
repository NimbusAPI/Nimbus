using System;
using System.Threading;
using System.Threading.Tasks;
using Apache.NMS;
using Nimbus.InfrastructureContracts;

namespace Nimbus.Transports.AMQP.ConnectionManagement
{
    internal class NmsConnectionManager : IDisposable
    {
        private readonly NmsConnectionFactory _connectionFactory;
        private readonly ILogger _logger;
        private readonly SemaphoreSlim _connectionLock = new SemaphoreSlim(1, 1);
        private IConnection _connection;
        private bool _disposed;

        public NmsConnectionManager(NmsConnectionFactory connectionFactory, ILogger logger)
        {
            _connectionFactory = connectionFactory;
            _logger = logger;
        }

        public async Task<ISession> CreateSession(AcknowledgementMode acknowledgementMode)
        {
            var connection = await GetOrCreateConnection();
            return await connection.CreateSessionAsync(acknowledgementMode);
        }

        public async Task TestConnection()
        {
            var connection = await GetOrCreateConnection();
            using var session = await connection.CreateSessionAsync();
            _logger.Debug("Test connection successful");
        }

        private async Task<IConnection> GetOrCreateConnection()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(NmsConnectionManager));

            if (_connection != null) return _connection;

            await _connectionLock.WaitAsync();
            try
            {
                if (_connection != null) return _connection;

                _connection = _connectionFactory.CreateConnection();
                _logger.Info("Created shared NMS connection");
                return _connection;
            }
            finally
            {
                _connectionLock.Release();
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            _logger.Info("Disposing connection manager");

            try
            {
                _connection?.Close();
                _connection?.Dispose();
            }
            catch (Exception ex)
            {
                _logger.Warn(ex, "Error disposing connection");
            }

            _connectionLock?.Dispose();
        }
    }
}
