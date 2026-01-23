using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Apache.NMS;
using Nimbus.InfrastructureContracts;

namespace Nimbus.Transports.AMQP.ConnectionManagement
{
    internal class NmsConnectionPool : IDisposable
    {
        private readonly NmsConnectionFactory _connectionFactory;
        private readonly AMQPTransportConfiguration _configuration;
        private readonly ILogger _logger;
        private readonly ConcurrentBag<IConnection> _connections;
        private readonly SemaphoreSlim _semaphore;
        private int _currentConnectionCount;
        private bool _disposed;

        public NmsConnectionPool(NmsConnectionFactory connectionFactory,
                                 AMQPTransportConfiguration configuration,
                                 ILogger logger)
        {
            _connectionFactory = connectionFactory;
            _configuration = configuration;
            _logger = logger;
            _connections = new ConcurrentBag<IConnection>();
            _semaphore = new SemaphoreSlim(_configuration.ConnectionPoolSize, _configuration.ConnectionPoolSize);
        }

        public async Task<PooledConnection> GetConnection()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(NmsConnectionPool));

            await _semaphore.WaitAsync();

            try
            {
                // Try to get an existing connection
                if (_connections.TryTake(out var connection))
                {
                    // Verify connection is still good
                    if (IsConnectionValid(connection))
                    {
                        return new PooledConnection(connection, this);
                    }
                    else
                    {
                        _logger.Warn("Discarding invalid connection from pool");
                        DisposeConnection(connection);
                        Interlocked.Decrement(ref _currentConnectionCount);
                    }
                }

                // Create a new connection
                var newConnection = _connectionFactory.CreateConnection();
                Interlocked.Increment(ref _currentConnectionCount);
                _logger.Debug("Connection pool now has {ConnectionCount} connections", _currentConnectionCount);

                return new PooledConnection(newConnection, this);
            }
            catch
            {
                _semaphore.Release();
                throw;
            }
        }

        internal void ReturnConnection(IConnection connection)
        {
            if (_disposed)
            {
                DisposeConnection(connection);
            }
            else if (IsConnectionValid(connection))
            {
                _connections.Add(connection);
            }
            else
            {
                _logger.Warn("Discarding invalid connection being returned to pool");
                DisposeConnection(connection);
                Interlocked.Decrement(ref _currentConnectionCount);
            }

            _semaphore.Release();
        }

        private bool IsConnectionValid(IConnection connection)
        {
            try
            {
                return connection != null; // && !connection.IsClosed;
            }
            catch
            {
                return false;
            }
        }

        private void DisposeConnection(IConnection connection)
        {
            try
            {
                connection?.Close();
                connection?.Dispose();
            }
            catch (Exception ex)
            {
                _logger.Warn(ex, "Error disposing connection");
            }
        }

        public async Task TestConnection()
        {
            using var pooledConnection = await GetConnection();
            
            // If we can get and create a session, connection is good
            using var session = await pooledConnection.Connection.CreateSessionAsync();
            _logger.Debug("Test connection successful");
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            _logger.Info("Disposing connection pool with {ConnectionCount} connections", _currentConnectionCount);

            while (_connections.TryTake(out var connection))
            {
                DisposeConnection(connection);
            }

            _semaphore?.Dispose();
        }
    }

    internal class PooledConnection : IDisposable
    {
        private readonly NmsConnectionPool _pool;
        private bool _disposed;

        public IConnection Connection { get; }

        public PooledConnection(IConnection connection, NmsConnectionPool pool)
        {
            Connection = connection;
            _pool = pool;
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _pool.ReturnConnection(Connection);
        }
    }
}
