namespace Nimbus.Transports.AzureServiceBus2.DelayedDelivery
{
    using System;
    using System.Threading.Tasks;
    using Nimbus.Infrastructure;
    using Nimbus.InfrastructureContracts;
    using Nimbus.Transports.AzureServiceBus2.BrokeredMessages;
    using Nimbus.Transports.AzureServiceBus2.QueueManagement;

    internal class DelayedDeliveryService : IDelayedDeliveryService
    {
        private readonly IQueueManager _queueManager;
        private readonly IBrokeredMessageFactory _brokeredMessageFactory;

        public DelayedDeliveryService(IQueueManager queueManager, IBrokeredMessageFactory brokeredMessageFactory)
        {
            this._queueManager = queueManager;
            this._brokeredMessageFactory = brokeredMessageFactory;
        }

        public async Task DeliverAfter(NimbusMessage message, DateTimeOffset deliveryTime)
        {
            if (await this._queueManager.QueueExists(message.DeliverTo))
            {
                message.DeliverAfter = deliveryTime;
                var messageSender = await this._queueManager.CreateMessageSender(message.DeliverTo);
                var message2 = await this._brokeredMessageFactory.BuildMessage(message);
                await messageSender.SendAsync(message2);
                return;
            }

            if (await this._queueManager.TopicExists(message.DeliverTo))
            {
                message.DeliverAfter = deliveryTime;
                var topicSender = await this._queueManager.CreateTopicSender(message.DeliverTo);
                var message2 = await this._brokeredMessageFactory.BuildMessage(message);
                await topicSender.SendAsync(message2);
                return;
            }

            throw new NotSupportedException("Message redelivery was requested but neither a queue path nor a topic path could be found.");
        }
    }
}