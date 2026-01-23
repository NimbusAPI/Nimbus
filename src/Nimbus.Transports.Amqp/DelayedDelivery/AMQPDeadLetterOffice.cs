using System;
using System.Threading.Tasks;
using Nimbus.Infrastructure;
using Nimbus.Infrastructure.Logging;
using Nimbus.InfrastructureContracts;

namespace Nimbus.Transports.AMQP.DelayedDelivery
{
    /// <summary>
    /// AMQP/Artemis has built-in Dead Letter Queue (DLQ) handling at the broker level.
    /// Messages that fail repeatedly are automatically moved to a DLQ by the broker.
    /// This implementation provides a way to manually send messages to a dead letter queue if needed.
    /// </summary>
    internal class AMQPDeadLetterOffice : IDeadLetterOffice
    {
        private const string DeadLetterQueuePrefix = "DLQ.";
        private readonly INimbusTransport _transport;
        private readonly ILogger _logger;

        public AMQPDeadLetterOffice(INimbusTransport transport, ILogger logger)
        {
            _transport = transport;
            _logger = logger;
        }


        public Task<NimbusMessage> Peek()
        {
            throw new NotImplementedException();
        }

        public Task<NimbusMessage> Pop()
        {
            throw new NotImplementedException();
        }

        public async Task Post(NimbusMessage message)
        {
            var deadLetterQueuePath = DeadLetterQueuePrefix + message.DeliverTo;

            _logger.Warn("Sending message {MessageId} to dead letter queue {DeadLetterQueue}",
                         message.MessageId, deadLetterQueuePath);

            try
            {
                var sender = _transport.GetQueueSender(deadLetterQueuePath);
                await sender.Send(message);

                _logger.Info("Message {MessageId} successfully moved to dead letter queue", message.MessageId);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to send message {MessageId} to dead letter queue", message.MessageId);
                throw;
            }
        }

        public Task<int> Count()
        {
            throw new NotImplementedException();
        }
    }
}
