using System.Threading.Tasks;
using Nimbus.Infrastructure;
using Nimbus.InfrastructureContracts;

namespace Nimbus.Transports.RabbitMQ.DelayedDelivery
{
    internal class RabbitMqDelayedDeliveryService : IDelayedDeliveryService
    {
        private readonly INimbusTransport _transport;
        private readonly ILogger _logger;

        public RabbitMqDelayedDeliveryService(INimbusTransport transport, ILogger logger)
        {
            _transport = transport;
            _logger = logger;
        }

        public async Task DeliverAfter(NimbusMessage message, System.DateTimeOffset deliveryTime)
        {
            message.DeliverAfter = deliveryTime;

            // For topic subscription retries, route directly to the subscription queue (bypassing the fanout exchange).
            // For command/request retries, route to the original queue.
            // In both cases GetQueueSender is correct — the sender will use the delayed exchange when DeliverAfter is in the future.
            string target = message.Properties.TryGetValue(MessagePropertyKeys.RedeliveryToSubscriptionName, out var subscriptionName)
                ? subscriptionName?.ToString()
                : message.DeliverTo;

            if (string.IsNullOrEmpty(target))
                throw new System.InvalidOperationException(
                    $"Cannot schedule message {message.MessageId}: DeliverTo is null and no {MessagePropertyKeys.RedeliveryToSubscriptionName} property is set.");

            _logger.Debug("Scheduling message {MessageId} for delivery at {DeliveryTime} to {Target}", message.MessageId, deliveryTime, target);

            var sender = _transport.GetQueueSender(target);
            await sender.Send(message);
        }
    }
}
