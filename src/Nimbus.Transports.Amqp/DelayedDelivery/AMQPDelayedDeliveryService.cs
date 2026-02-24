using System;
using System.Threading.Tasks;
using Nimbus.Infrastructure;
using Nimbus.Infrastructure.MessageSendersAndReceivers;
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

            // If the message came from a topic subscription, re-publish to the topic.
            // The RedeliveryToSubscriptionName property (carried as an NMS message property)
            // will be matched by the JMS selector on the originating subscription so only
            // that subscription receives the retry.
            INimbusMessageSender sender;
            if (message.Properties.ContainsKey(MessagePropertyKeys.RedeliveryToSubscriptionName))
            {
                sender = _transport.GetTopicSender(message.DeliverTo);
            }
            else
            {
                sender = _transport.GetQueueSender(message.DeliverTo);
            }

            await sender.Send(message);

            _logger.Debug("Message {MessageId} scheduled successfully", message.MessageId);
        }
    }
}
