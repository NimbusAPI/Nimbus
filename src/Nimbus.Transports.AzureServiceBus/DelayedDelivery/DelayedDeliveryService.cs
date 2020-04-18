using System;
using System.Threading.Tasks;
using Nimbus.Infrastructure;
using Nimbus.InfrastructureContracts;
using Nimbus.Transports.AzureServiceBus.Messages;
using Nimbus.Transports.AzureServiceBus.QueueManagement;

namespace Nimbus.Transports.AzureServiceBus.DelayedDelivery
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
                var Message = await _brokeredMessageFactory.BuildMessage(message);
                await messageSender.SendAsync(Message);
                return;
            }

            if (await _queueManager.TopicExists(message.DeliverTo))
            {
                message.DeliverAfter = deliveryTime;
                var topicSender = await _queueManager.CreateTopicSender(message.DeliverTo);
                var Message2 = await _brokeredMessageFactory.BuildMessage(message);
                await topicSender.SendAsync(Message2);
                return;
            }

            throw new NotSupportedException("Message redelivery was requested but neither a queue path nor a topic path could be found.");
        }
    }
}