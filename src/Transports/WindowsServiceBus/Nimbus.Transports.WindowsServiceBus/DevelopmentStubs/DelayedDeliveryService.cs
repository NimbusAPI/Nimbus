using System;
using System.Threading.Tasks;
using Nimbus.Infrastructure;
using Nimbus.Transports.WindowsServiceBus.BrokeredMessages;

namespace Nimbus.Transports.WindowsServiceBus.DevelopmentStubs
{
    internal class DelayedDeliveryService : IDelayedDeliveryService
    {
        private readonly IQueueManager _queueManager;
        private readonly IBrokeredMessageFactory _brokeredMessageFactory;

        public DelayedDeliveryService(IQueueManager queueManager, IBrokeredMessageFactory brokeredMessageFactory)
        {
            _queueManager = queueManager;
            _brokeredMessageFactory = brokeredMessageFactory;
        }

        public async Task DeliverAt(NimbusMessage message, DateTimeOffset deliveryTime)
        {
            var messageSender = await _queueManager.CreateMessageSender(message.To);
            var brokeredMessage = _brokeredMessageFactory.BuildBrokeredMessage(message);
            await messageSender.SendAsync(brokeredMessage);
        }
    }
}