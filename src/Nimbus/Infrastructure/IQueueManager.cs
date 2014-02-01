using System;
using Microsoft.ServiceBus.Messaging;

namespace Nimbus.Infrastructure
{
    public interface IQueueManager
    {
        MessageSender CreateMessageSender(Type messageType);
        MessageReceiver CreateMessageReceiver(string queuePath);

        SubscriptionClient CreateSubscriptionReceiver(string topicPath, string subscriptionName);

        QueueClient CreateDeadLetterQueueClient<T>();
    }
}