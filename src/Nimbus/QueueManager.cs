using System;
using Microsoft.ServiceBus;

namespace Nimbus
{
    public class QueueManager : IQueueManager
    {
        private readonly NamespaceManager _namespaceManager;

        public QueueManager(NamespaceManager namespaceManager)
        {
            _namespaceManager = namespaceManager;
        }

        public void EnsureSubscriptionExists(Type eventType, string subscriptionName)
        {
            if (_namespaceManager.SubscriptionExists(eventType.FullName, subscriptionName)) return;

            _namespaceManager.CreateSubscription(eventType.FullName, subscriptionName);
        }

        public void EnsureTopicExists(Type eventType)
        {
            var topicName = eventType.FullName;
            if (_namespaceManager.TopicExists(topicName)) return;

            _namespaceManager.CreateTopic(topicName);
        }

        public void EnsureQueueExists(Type commandType)
        {
            EnsureQueueExists(commandType.FullName);
        }

        public void EnsureQueueExists(string queueName)
        {
            if (_namespaceManager.QueueExists(queueName)) return;

            _namespaceManager.CreateQueue(queueName);
        }
    }
}