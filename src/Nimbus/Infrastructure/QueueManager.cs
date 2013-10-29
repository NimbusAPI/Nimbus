using System;
using System.Collections.Concurrent;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;

namespace Nimbus.Infrastructure
{
    public class QueueManager : IQueueManager
    {
        private readonly NamespaceManager _namespaceManager;
        private readonly MessagingFactory _messagingFactory;
        private readonly int _maxDeliveryAttempts;

        private readonly ConcurrentDictionary<Type, QueueClient> _queueClients = new ConcurrentDictionary<Type, QueueClient>();
        private readonly ConcurrentDictionary<Type, QueueClient> _deadLetterQueueClients = new ConcurrentDictionary<Type, QueueClient>();

        public QueueManager(NamespaceManager namespaceManager, MessagingFactory messagingFactory, int maxDeliveryAttempts)
        {
            _namespaceManager = namespaceManager;
            _messagingFactory = messagingFactory;
            _maxDeliveryAttempts = maxDeliveryAttempts;
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
            EnsureQueueExists(GetQueueName(commandType));
        }

        private static string GetQueueName(Type messageContractType)
        {
            var queueName = messageContractType.FullName;
            return queueName;
        }

        private string GetDeadLetterQueueName(Type messageContractType)
        {
            var queueName = messageContractType.FullName;
            var deadLetterQueueName = QueueClient.FormatDeadLetterPath(queueName);
            return deadLetterQueueName;
        }

        public void EnsureQueueExists(string queueName)
        {
            var queueDescription = new QueueDescription(queueName)
            {
                MaxDeliveryCount = _maxDeliveryAttempts,
                DefaultMessageTimeToLive = TimeSpan.MaxValue,
                EnableDeadLetteringOnMessageExpiration = true,
                EnableBatchedOperations = true,
                LockDuration = TimeSpan.FromSeconds(30),
                RequiresDuplicateDetection = false,
                RequiresSession = false,
                SupportOrdering = false,
            };

            if (_namespaceManager.QueueExists(queueName))
            {
                _namespaceManager.UpdateQueue(queueDescription);
            }
            else
            {
                _namespaceManager.CreateQueue(queueDescription);
            }
        }

        public QueueClient CreateQueueClient<T>()
        {
            var messageContractType = typeof (T);
            var queueName = GetQueueName(messageContractType);

            return _queueClients.GetOrAdd(messageContractType, t => GetOrCreateQueue(messageContractType, queueName));
        }

        public QueueClient CreateDeadLetterQueueClient<T>()
        {
            var messageContractType = typeof (T);
            var queueName = GetDeadLetterQueueName(messageContractType);

            return _deadLetterQueueClients.GetOrAdd(messageContractType, t => GetOrCreateQueue(messageContractType, queueName));
        }

        private QueueClient GetOrCreateQueue(Type messageContractType, string queueName)
        {
            EnsureQueueExists(messageContractType);
            return _messagingFactory.CreateQueueClient(queueName);
        }
    }
}