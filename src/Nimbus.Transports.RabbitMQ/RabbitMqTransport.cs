using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Nimbus.Configuration.PoorMansIocContainer;
using Nimbus.Infrastructure;
using Nimbus.Infrastructure.MessageSendersAndReceivers;
using Nimbus.InfrastructureContracts;
using Nimbus.InfrastructureContracts.Filtering.Conditions;
using Nimbus.Transports.RabbitMQ.ConnectionManagement;
using Nimbus.Transports.RabbitMQ.MessageSendersAndReceivers;

namespace Nimbus.Transports.RabbitMQ
{
    internal class RabbitMqTransport : INimbusTransport, IDisposable
    {
        private readonly PoorMansIoC _container;
        private readonly ILogger _logger;
        private readonly RabbitMqConnectionManager _connectionManager;
        private readonly ConcurrentDictionary<string, RabbitMqQueueSender> _queueSenders = new();
        private readonly ConcurrentDictionary<string, RabbitMqTopicSender> _topicSenders = new();
        private bool _disposed;

        public RabbitMqTransport(PoorMansIoC container, ILogger logger, RabbitMqConnectionManager connectionManager)
        {
            _container = container;
            _logger = logger;
            _connectionManager = connectionManager;
        }

        public async Task TestConnection()
        {
            await _connectionManager.TestConnectionAsync();
        }

        public INimbusMessageSender GetQueueSender(string queuePath)
        {
            return _queueSenders.GetOrAdd(queuePath, path =>
                _container.ResolveWithOverrides<RabbitMqQueueSender>(path));
        }

        public INimbusMessageReceiver GetQueueReceiver(string queuePath)
        {
            return _container.ResolveWithOverrides<RabbitMqQueueReceiver>(queuePath);
        }

        public INimbusMessageSender GetTopicSender(string topicPath)
        {
            return _topicSenders.GetOrAdd(topicPath, path =>
                _container.ResolveWithOverrides<RabbitMqTopicSender>(path));
        }

        public INimbusMessageReceiver GetTopicReceiver(string topicPath, string subscriptionName, IFilterCondition filter)
        {
            var subscription = new RabbitMqSubscription(topicPath, subscriptionName);
            return _container.ResolveWithOverrides<RabbitMqTopicReceiver>(subscription, filter);
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            foreach (var sender in _queueSenders.Values)
                try { sender.DisposeAsync().AsTask().GetAwaiter().GetResult(); } catch { /* ignore */ }

            foreach (var sender in _topicSenders.Values)
                try { sender.DisposeAsync().AsTask().GetAwaiter().GetResult(); } catch { /* ignore */ }

            try { _connectionManager.DisposeAsync().AsTask().GetAwaiter().GetResult(); } catch { /* ignore */ }
        }
    }
}
