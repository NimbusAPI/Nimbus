using System;
using System.Threading.Tasks;
using Nimbus.Infrastructure;
using Nimbus.Infrastructure.Logging;
using Nimbus.InfrastructureContracts;

namespace Nimbus.Transports.AMQP.DelayedDelivery
{
    /// <summary>
    /// AMQP/Artemis supports delayed delivery natively via the _AMQ_SCHED_DELIVERY property.
    /// This service just ensures the message has the correct DeliverAfter timestamp set,
    /// which will be converted to the scheduling property by the message factory.
    /// </summary>
    internal class AMQPDelayedDeliveryService : IDelayedDeliveryService
    {
        private readonly INimbusTransport _transport;
        private readonly ILogger _logger;

        public AMQPDelayedDeliveryService(INimbusTransport transport, ILogger logger)
        {
            _transport = transport;
            _logger = logger;
        }

        public async Task DeliverAfter(NimbusMessage message, DateTimeOffset deliveryTime)
        {
            // Set the DeliverAfter property - the message factory will convert this
            // to the _AMQ_SCHED_DELIVERY property when creating the NMS message
            message.DeliverAfter = deliveryTime;

            _logger.Debug("Scheduling message {MessageId} for delivery at {DeliveryTime} to {DeliverTo}",
                message.MessageId, deliveryTime, message.DeliverTo);

            // Send to the queue - AMQP will hold it until the scheduled time
            var sender = _transport.GetQueueSender(message.DeliverTo);
            await sender.Send(message);

            _logger.Debug("Message {MessageId} scheduled successfully", message.MessageId);
        }
    }
}
