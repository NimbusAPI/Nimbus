using System;
using System.Threading.Tasks;
using Nimbus.Infrastructure;
using Nimbus.InfrastructureContracts;
using static Nimbus.Infrastructure.MessagePropertyKeys;

namespace Nimbus.Transports.Postgres.DelayedDelivery
{
    internal class PostgresDelayedDeliveryService : IDelayedDeliveryService
    {
        private readonly INimbusTransport _transport;
        private readonly ILogger _logger;

        public PostgresDelayedDeliveryService(INimbusTransport transport, ILogger logger)
        {
            _transport = transport;
            _logger = logger;
        }

        public async Task DeliverAfter(NimbusMessage message, DateTimeOffset deliveryTime)
        {
            // If this message was received from a topic subscription, route it directly
            // back to that subscriber queue. Using GetQueueSender(message.DeliverTo) would
            // point at the topic path and the message would never be picked up (no receiver
            // polls the raw topic path). A queue send also avoids fan-out to other subscribers.
            var destination = message.Properties.TryGetValue(RedeliveryToSubscriptionName, out var subscriberQueue)
                ? (string) subscriberQueue
                : message.DeliverTo;

            _logger.Debug("Re-enqueuing {MessageId} for delivery at {DeliveryTime} on {Queue}",
                message.MessageId,
                deliveryTime,
                destination);

            message.DeliverAfter = deliveryTime;

            var sender = _transport.GetQueueSender(destination);
            await sender.Send(message);
        }
    }
}
