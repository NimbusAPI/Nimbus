using System;
using System.Threading.Tasks;
using Nimbus.Configuration.PoorMansIocContainer;
using Nimbus.Infrastructure;
using Nimbus.Infrastructure.MessageSendersAndReceivers;
using Nimbus.InfrastructureContracts;
using Nimbus.InfrastructureContracts.Filtering.Conditions;
using Nimbus.Transports.AMQP.ConnectionManagement;
using Nimbus.Transports.AMQP.MessageSendersAndReceivers;
using Nimbus.Transports.AMQP.QueueManagement;

namespace Nimbus.Transports.AMQP
{
    internal class AMQPTransport : INimbusTransport, IDisposable
    {
        private readonly PoorMansIoC _container;
        private readonly ILogger _logger;
        private readonly NmsConnectionManager _connectionManager;
        private bool _disposed;

        public AMQPTransport(PoorMansIoC container, ILogger logger, NmsConnectionManager connectionManager)
        {
            _container = container;
            _logger = logger;
            _connectionManager = connectionManager;
        }

        public async Task TestConnection()
        {
            _logger.Debug("Testing AMQP connection");
            await _connectionManager.TestConnection();
            _logger.Info("AMQP connection test successful");
        }

        public INimbusMessageSender GetQueueSender(string queuePath)
        {
            _logger.Debug("Creating queue sender for {QueuePath}", queuePath);
            return _container.ResolveWithOverrides<AMQPQueueSender>(queuePath);
        }

        public INimbusMessageReceiver GetQueueReceiver(string queuePath)
        {
            _logger.Debug("Creating queue receiver for {QueuePath}", queuePath);
            return _container.ResolveWithOverrides<AMQPQueueReceiver>(queuePath);
        }

        public INimbusMessageSender GetTopicSender(string topicPath)
        {
            _logger.Debug("Creating topic sender for {TopicPath}", topicPath);
            return _container.ResolveWithOverrides<AMQPTopicSender>(topicPath);
        }

        public INimbusMessageReceiver GetTopicReceiver(string topicPath, string subscriptionName, IFilterCondition filter)
        {
            _logger.Debug("Creating topic receiver for {TopicPath} with subscription {SubscriptionName}",
                topicPath, subscriptionName);
            var subscription = new Subscription(topicPath, subscriptionName);
            return _container.ResolveWithOverrides<AMQPTopicReceiver>(subscription, filter);
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            _logger.Info("Disposing AMQP transport");

            try
            {
                _connectionManager?.Dispose();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error disposing connection manager");
            }
        }
    }
}
