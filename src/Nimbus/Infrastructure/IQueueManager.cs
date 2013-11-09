using System;
using Microsoft.ServiceBus.Messaging;

namespace Nimbus.Infrastructure
{
    internal interface IQueueManager
    {
        void EnsureSubscriptionExists(Type eventType, string subscriptionName);
        void EnsureTopicExists(Type eventType);
        void EnsureQueueExists(Type commandType);
        void EnsureQueueExists(string queueName);

        QueueClient CreateDeadLetterQueueClient<T>();
    }
}