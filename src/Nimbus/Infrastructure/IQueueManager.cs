using System;

namespace Nimbus.Infrastructure
{
    public interface IQueueManager
    {
        void EnsureSubscriptionExists(Type eventType, string subscriptionName);
        void EnsureTopicExists(Type eventType);
        void EnsureQueueExists(Type commandType);
        void EnsureQueueExists(string queueName);
    }
}