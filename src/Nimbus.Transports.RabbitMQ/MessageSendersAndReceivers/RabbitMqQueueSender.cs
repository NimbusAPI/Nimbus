using System.Threading.Tasks;
using Nimbus.Infrastructure;
using Nimbus.InfrastructureContracts;
using Nimbus.Transports.RabbitMQ.ConnectionManagement;
using Nimbus.Transports.RabbitMQ.MessageConversion;
using RabbitMQ.Client;

namespace Nimbus.Transports.RabbitMQ.MessageSendersAndReceivers
{
    internal class RabbitMqQueueSender : RabbitMqSenderBase
    {
        private readonly string _queuePath;

        public RabbitMqQueueSender(string queuePath,
                                    RabbitMqConnectionManager connectionManager,
                                    RabbitMqMessageConverter messageConverter,
                                    IClock clock,
                                    ILogger logger)
            : base(connectionManager, messageConverter, clock, logger)
        {
            _queuePath = queuePath;
        }

        protected override string SenderDescription => $"queue {_queuePath}";

        protected override async Task EnsureChannelAsync()
        {
            if (_channel != null) return;

            _channel = await _connectionManager.CreateChannelAsync();
            await _connectionManager.EnsureDelayedExchangeDeclaredAsync(_channel);
            await _channel.QueueDeclareAsync(_queuePath, durable: true, exclusive: false, autoDelete: false);
            await _channel.QueueBindAsync(_queuePath, RabbitMqConnectionManager.DelayedExchangeName, routingKey: _queuePath);
        }

        protected override async Task PublishAsync(NimbusMessage message, byte[] body, BasicProperties props)
        {
            bool isDelayed = message.DeliverAfter > _clock.UtcNow;
            string exchange = isDelayed ? RabbitMqConnectionManager.DelayedExchangeName : "";

            _logger.Debug("Sending message {MessageId} to queue {QueuePath} (delayed={IsDelayed})", message.MessageId, _queuePath, isDelayed);
            await _channel.BasicPublishAsync(exchange, _queuePath, mandatory: false, basicProperties: props, body: body);
        }
    }
}
