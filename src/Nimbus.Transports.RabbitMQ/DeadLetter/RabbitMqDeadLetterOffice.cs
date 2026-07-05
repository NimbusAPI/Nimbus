using System;
using System.Threading.Tasks;
using Nimbus.Infrastructure;
using Nimbus.InfrastructureContracts;
using Nimbus.Transports.RabbitMQ.ConnectionManagement;
using Nimbus.Transports.RabbitMQ.MessageConversion;
using RabbitMQ.Client;

namespace Nimbus.Transports.RabbitMQ.DeadLetter
{
    internal class RabbitMqDeadLetterOffice : IDeadLetterOffice
    {
        private const string DeadLetterQueueName = "nimbus.deadletter";

        private readonly INimbusTransport _transport;
        private readonly RabbitMqConnectionManager _connectionManager;
        private readonly RabbitMqMessageConverter _messageConverter;
        private readonly ILogger _logger;

        public RabbitMqDeadLetterOffice(INimbusTransport transport,
                                        RabbitMqConnectionManager connectionManager,
                                        RabbitMqMessageConverter messageConverter,
                                        ILogger logger)
        {
            _transport = transport;
            _connectionManager = connectionManager;
            _messageConverter = messageConverter;
            _logger = logger;
        }

        public async Task Post(NimbusMessage message)
        {
            _logger.Debug("Posting message {MessageId} to dead letter queue", message.MessageId);
            await _transport.GetQueueSender(DeadLetterQueueName).Send(message);
        }

        public async Task<NimbusMessage> Pop()
        {
            IChannel channel = null;
            try
            {
                channel = await _connectionManager.CreateChannelAsync();
                await channel.QueueDeclareAsync(DeadLetterQueueName, durable: true, exclusive: false, autoDelete: false);
                var result = await channel.BasicGetAsync(DeadLetterQueueName, autoAck: false);
                if (result == null) return null;

                try
                {
                    var message = _messageConverter.FromRabbitMq(result.Body);
                    await channel.BasicAckAsync(result.DeliveryTag, multiple: false);
                    return message;
                }
                catch
                {
                    try { await channel.BasicNackAsync(result.DeliveryTag, multiple: false, requeue: false); } catch { }
                    throw;
                }
            }
            finally
            {
                await CloseChannelAsync(channel);
            }
        }

        public async Task<NimbusMessage> Peek()
        {
            IChannel channel = null;
            try
            {
                channel = await _connectionManager.CreateChannelAsync();
                await channel.QueueDeclareAsync(DeadLetterQueueName, durable: true, exclusive: false, autoDelete: false);
                var result = await channel.BasicGetAsync(DeadLetterQueueName, autoAck: false);
                if (result == null) return null;

                await channel.BasicNackAsync(result.DeliveryTag, multiple: false, requeue: true);
                return _messageConverter.FromRabbitMq(result.Body);
            }
            finally
            {
                await CloseChannelAsync(channel);
            }
        }

        public async Task<int> Count()
        {
            IChannel channel = null;
            try
            {
                channel = await _connectionManager.CreateChannelAsync();
                var ok = await channel.QueueDeclareAsync(DeadLetterQueueName, durable: true, exclusive: false, autoDelete: false);
                return (int)ok.MessageCount;
            }
            catch (Exception ex)
            {
                _logger.Warn(ex, "Could not count dead letter queue messages");
                return 0;
            }
            finally
            {
                await CloseChannelAsync(channel);
            }
        }

        private async Task CloseChannelAsync(IChannel channel)
        {
            if (channel == null) return;
            try { await channel.CloseAsync(); } catch { /* ignore */ }
            try { channel.Dispose(); } catch { /* ignore */ }
        }
    }
}
