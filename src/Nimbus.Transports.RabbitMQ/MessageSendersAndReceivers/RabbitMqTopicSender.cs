using System.Threading.Tasks;
using Nimbus.Infrastructure;
using Nimbus.InfrastructureContracts;
using Nimbus.Transports.RabbitMQ.ConnectionManagement;
using Nimbus.Transports.RabbitMQ.MessageConversion;
using RabbitMQ.Client;

namespace Nimbus.Transports.RabbitMQ.MessageSendersAndReceivers
{
    internal class RabbitMqTopicSender : RabbitMqSenderBase
    {
        private readonly string _topicPath;

        public RabbitMqTopicSender(string topicPath,
                                    RabbitMqConnectionManager connectionManager,
                                    RabbitMqMessageConverter messageConverter,
                                    IClock clock,
                                    ILogger logger)
            : base(connectionManager, messageConverter, clock, logger)
        {
            _topicPath = topicPath;
        }

        protected override string SenderDescription => $"topic exchange {_topicPath}";

        protected override async Task EnsureChannelAsync()
        {
            if (_channel != null) return;

            _channel = await _connectionManager.CreateChannelAsync();
            await _connectionManager.EnsureDelayedExchangeDeclaredAsync(_channel);
            await _channel.ExchangeDeclareAsync(_topicPath, type: "fanout", durable: true, autoDelete: false);
        }

        protected override async Task PublishAsync(NimbusMessage message, byte[] body, BasicProperties props)
        {
            if (message.DeliverAfter > _clock.UtcNow)
            {
                // Delayed publishes go through the x-delayed-message exchange with the topic path as routing key.
                // Topic receivers bind their subscription queues to this exchange with routingKey=_topicPath.
                _logger.Debug("Publishing delayed message {MessageId} to topic {TopicPath}", message.MessageId, _topicPath);
                await _channel.BasicPublishAsync(RabbitMqConnectionManager.DelayedExchangeName, _topicPath, mandatory: false, basicProperties: props, body: body);
            }
            else
            {
                _logger.Debug("Publishing message {MessageId} to topic exchange {TopicPath}", message.MessageId, _topicPath);
                await _channel.BasicPublishAsync(_topicPath, routingKey: "", mandatory: false, basicProperties: props, body: body);
            }
        }
    }
}
