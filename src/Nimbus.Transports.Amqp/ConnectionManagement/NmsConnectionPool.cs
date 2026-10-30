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
        // NMS tracks durable subscriptions per connection in an unsynchronised HashSet, so durable consumers on the
        // shared connection must be created and closed one at a time. Message pumps start in parallel, which races it.
        private readonly SemaphoreSlim _durableSubscriptionLock = new SemaphoreSlim(1, 1);
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

        public async Task<IMessageConsumer> CreateDurableConsumer(ISession session, ITopic topic, string subscriptionName, string selector)
        {
            await _durableSubscriptionLock.WaitAsync();
            try
            {
                return await session.CreateDurableConsumerAsync(topic, subscriptionName, selector, false);
            }
            finally
            {
                _durableSubscriptionLock.Release();
            }
        }

        public void CloseDurableConsumer(IMessageConsumer consumer)
        {
            _durableSubscriptionLock.Wait();
            try
            {
                consumer.Close();
            }
            finally
            {
                _durableSubscriptionLock.Release();
            }
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
            _durableSubscriptionLock?.Dispose();
        }
    }
}
