using System;
using Microsoft.ServiceBus;

namespace Nimbus
{
    public class QueueManager : IQueueManager
    {
        private readonly NamespaceManager _namespaceManager;

        public QueueManager(string connectionString)
        {
            _namespaceManager = NamespaceManager.CreateFromConnectionString(connectionString);
        }

        public void EnsureSubscriptionExists(Type eventType, string subscriptionName)
        {
            if (!_namespaceManager.SubscriptionExists(eventType.FullName, subscriptionName))
            {
                _namespaceManager.CreateSubscription(eventType.FullName, subscriptionName);
            }
        }

        public void EnsureTopicExists(Type eventType)
        {
            var topicName = eventType.FullName;

            if (!_namespaceManager.TopicExists(topicName))
            {
                _namespaceManager.CreateTopic(topicName);
            }
        }

        public void EnsureQueueExists(Type commandType)
        {
            EnsureQueueExists(commandType.FullName);
        }

        public void EnsureQueueExists(string queueName)
        {
            if (!_namespaceManager.QueueExists(queueName))
            {
                _namespaceManager.CreateQueue(queueName);
            }
        }
    }
}