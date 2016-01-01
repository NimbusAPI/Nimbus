using System;
using System.Threading.Tasks;
using Nimbus.Infrastructure;
using Nimbus.Transports.WindowsServiceBus.BrokeredMessages;
using Nimbus.Transports.WindowsServiceBus.QueueManagement;

namespace Nimbus.Transports.WindowsServiceBus.DelayedDelivery
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

        public async Task DeliverAfter(NimbusMessage message, DateTimeOffset deliveryTime)
        {
            message.DeliverAfter = deliveryTime;
            var messageSender = await _queueManager.CreateMessageSender(message.To);
            var brokeredMessage = await _brokeredMessageFactory.BuildBrokeredMessage(message);
            await messageSender.SendAsync(brokeredMessage);
        }
    }
}