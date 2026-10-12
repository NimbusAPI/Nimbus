using System.Threading.Tasks;
using Nimbus.Infrastructure;
using Nimbus.InfrastructureContracts;

namespace Nimbus.Transports.RabbitMQ.DelayedDelivery
{
    internal class RabbitMqDelayedDeliveryService : IDelayedDeliveryService
    {
        private readonly RabbitMqTransport _transport;
        private readonly ILogger _logger;

        public RabbitMqDelayedDeliveryService(RabbitMqTransport transport, ILogger logger)
        {
            _transport = transport;
            _logger = logger;
        }

        public async Task DeliverAfter(NimbusMessage message, System.DateTimeOffset deliveryTime)
        {
            message.DeliverAfter = deliveryTime;

            // Topic subscription retries go straight to that subscription's queue (bypassing the fanout exchange),
            // using a sender that declares the queue exactly as the subscription receiver does.
            // Command/request retries go to the original queue.
            // Either sender uses the delayed exchange when DeliverAfter is in the future.
            if (message.Properties.TryGetValue(MessagePropertyKeys.RedeliveryToSubscriptionName, out var subscriptionName)
                && !string.IsNullOrEmpty(subscriptionName?.ToString()))
            {
                _logger.Debug("Scheduling message {MessageId} for delivery at {DeliveryTime} to subscription {Subscription}", message.MessageId, deliveryTime, subscriptionName);
                await _transport.GetSubscriptionRetrySender(subscriptionName.ToString()).Send(message);
                return;
            }

            if (string.IsNullOrEmpty(message.DeliverTo))
                throw new System.InvalidOperationException(
                    $"Cannot schedule message {message.MessageId}: DeliverTo is null and no {MessagePropertyKeys.RedeliveryToSubscriptionName} property is set.");

            _logger.Debug("Scheduling message {MessageId} for delivery at {DeliveryTime} to {Target}", message.MessageId, deliveryTime, message.DeliverTo);
            await _transport.GetQueueSender(message.DeliverTo).Send(message);
        }
    }
}
