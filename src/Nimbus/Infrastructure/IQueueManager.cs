using Microsoft.ServiceBus.Messaging;

namespace Nimbus.Infrastructure
{
    internal interface IQueueManager
    {
        MessageSender CreateMessageSender(string queuePath);
        MessageReceiver CreateMessageReceiver(string queuePath);

        SubscriptionClient CreateSubscriptionReceiver(string topicPath, string subscriptionName);

        QueueClient CreateDeadLetterQueueClient<T>();
    }
}