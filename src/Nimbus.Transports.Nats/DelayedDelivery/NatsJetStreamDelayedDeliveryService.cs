using System;
using System.Text;
using System.Threading.Tasks;
using NATS.Client.Core;
using NATS.Client.JetStream.Models;
using Nimbus.Infrastructure;
using Nimbus.InfrastructureContracts;
using Nimbus.Transports.Nats.ConnectionManagement;

namespace Nimbus.Transports.Nats.DelayedDelivery
{
    internal class NatsJetStreamDelayedDeliveryService : IDelayedDeliveryService
    {
        private readonly NatsJetStreamContextFactory _jsContextFactory;
        private readonly ISerializer _serializer;
        private readonly ILogger _logger;

        public NatsJetStreamDelayedDeliveryService(NatsJetStreamContextFactory jsContextFactory,
                                                   ISerializer serializer,
                                                   ILogger logger)
        {
            _jsContextFactory = jsContextFactory;
            _serializer = serializer;
            _logger = logger;
        }

        public async Task DeliverAfter(NimbusMessage message, DateTimeOffset deliveryTime)
        {
            // For topic subscribers, RedeliveryToSubscriptionName is the per-subscription retry
            // subject (a workqueue). For queue messages it falls back to DeliverTo.
            var destination = message.Properties.TryGetValue(MessagePropertyKeys.RedeliveryToSubscriptionName, out var sub)
                ? (string)sub!
                : message.DeliverTo;

            _logger.Debug("Scheduling {MessageId} for JetStream delivery at {DeliverAt} via {Destination}",
                message.MessageId, deliveryTime, destination);

            var streamName = $"Q_{SanitiseName(destination)}";
            await _jsContextFactory.EnsureStreamAsync(streamName, destination, StreamConfigRetention.Workqueue);

            var headers = new NatsHeaders();
            headers.Add("Nats-Schedule", "@at " + deliveryTime.UtcDateTime.ToString("o"));
            headers.Add("Nats-Schedule-Target", destination);

            var bytes = Encoding.UTF8.GetBytes(_serializer.Serialize(message));
            await _jsContextFactory.PublishAsync(destination + ".sched", bytes, headers);
        }

        private static string SanitiseName(string path) => NatsNameSanitiser.Sanitise(path);
    }
}
