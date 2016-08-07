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
            if (await _queueManager.QueueExists(message.DeliverTo))
            {
                message.DeliverAfter = deliveryTime;
                var messageSender = await _queueManager.CreateMessageSender(message.DeliverTo);
                var brokeredMessage = await _brokeredMessageFactory.BuildBrokeredMessage(message);
                await messageSender.SendAsync(brokeredMessage);
                return;
            }

            if (await _queueManager.TopicExists(message.DeliverTo))
            {
                message.DeliverAfter = deliveryTime;
                var topicSender = await _queueManager.CreateTopicSender(message.DeliverTo);
                var brokeredMessage2 = await _brokeredMessageFactory.BuildBrokeredMessage(message);
                await topicSender.SendAsync(brokeredMessage2);
                return;
            }

            throw new NotSupportedException("Message redelivery was requested but neither a queue path nor a topic path could be found.");
        }
    }
}