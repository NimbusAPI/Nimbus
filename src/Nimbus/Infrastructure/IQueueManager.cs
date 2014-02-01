using System;
using Microsoft.ServiceBus.Messaging;

namespace Nimbus.Infrastructure
{
    public interface IQueueManager
    {
        void EnsureSubscriptionExists(Type eventType, string subscriptionName);
        void EnsureTopicExists(Type eventType);
        void EnsureQueueExists(Type commandType);
        void EnsureQueueExists(string queuePath);

        MessageSender CreateMessageSender(Type messageType);

        QueueClient CreateDeadLetterQueueClient<T>();
    }
}