using System.Threading.Tasks;
using Nimbus.Configuration.Settings;
using Nimbus.Infrastructure;
using Nimbus.InfrastructureContracts;
using Nimbus.Transports.RabbitMQ.ConnectionManagement;
using Nimbus.Transports.RabbitMQ.MessageConversion;
using RabbitMQ.Client;

namespace Nimbus.Transports.RabbitMQ.MessageSendersAndReceivers
{
    // Sends a retry straight to one subscription's queue. The queue is declared with the same arguments as
    // RabbitMqTopicReceiver, so whichever side declares it first, the other's declaration matches.
    internal class RabbitMqSubscriptionRetrySender : RabbitMqSenderBase
    {
        private readonly string _subscriptionName;
        private readonly AutoDeleteOnIdleSetting _autoDeleteOnIdle;

        public RabbitMqSubscriptionRetrySender(string subscriptionName,
                                               AutoDeleteOnIdleSetting autoDeleteOnIdle,
                                               RabbitMqConnectionManager connectionManager,
                                               RabbitMqMessageConverter messageConverter,
                                               IClock clock,
                                               ILogger logger)
            : base(connectionManager, messageConverter, clock, logger)
        {
            _subscriptionName = subscriptionName;
            _autoDeleteOnIdle = autoDeleteOnIdle;
        }

        protected override string SenderDescription => $"subscription queue {_subscriptionName}";

        protected override async Task EnsureChannelAsync()
        {
            if (_channel != null) return;

            _channel = await _connectionManager.CreateChannelAsync();
            await _connectionManager.EnsureDelayedExchangeDeclaredAsync(_channel);
            _channel = await RabbitMqSubscriptionQueue.DeclareAsync(_connectionManager, _channel, _subscriptionName, _autoDeleteOnIdle.Value, _logger);
        }

        protected override async Task PublishAsync(NimbusMessage message, byte[] body, BasicProperties props)
        {
            bool isDelayed = message.DeliverAfter > _clock.UtcNow;
            string exchange = isDelayed ? RabbitMqConnectionManager.DelayedExchangeName : "";

            _logger.Debug("Sending message {MessageId} to subscription queue {SubscriptionName} (delayed={IsDelayed})", message.MessageId, _subscriptionName, isDelayed);
            await _channel.BasicPublishAsync(exchange, _subscriptionName, mandatory: false, basicProperties: props, body: body);
        }
    }
}
