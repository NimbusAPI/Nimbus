using System;
using System.Collections.Concurrent;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Nimbus.InfrastructureContracts;

namespace Nimbus.Infrastructure
{
    public class QueueManager : IQueueManager
    {
        private readonly NamespaceManager _namespaceManager;
        private readonly MessagingFactory _messagingFactory;
        private readonly int _maxDeliveryAttempts;
        private readonly ILogger _logger;

        private readonly ConcurrentDictionary<Type, QueueClient> _queueClients = new ConcurrentDictionary<Type, QueueClient>();
        private readonly ConcurrentDictionary<Type, QueueClient> _deadLetterQueueClients = new ConcurrentDictionary<Type, QueueClient>();

        public QueueManager(NamespaceManager namespaceManager, MessagingFactory messagingFactory, int maxDeliveryAttempts, ILogger logger)
        {
            _namespaceManager = namespaceManager;
            _messagingFactory = messagingFactory;
            _maxDeliveryAttempts = maxDeliveryAttempts;
            _logger = logger;
        }

        public void EnsureSubscriptionExists(Type eventType, string subscriptionName)
        {
            var topicPath = PathFactory.TopicPathFor(eventType);

            var subscriptionDescription = new SubscriptionDescription(topicPath, subscriptionName)
            {
                MaxDeliveryCount = _maxDeliveryAttempts,
                DefaultMessageTimeToLive = TimeSpan.MaxValue,
                EnableDeadLetteringOnMessageExpiration = true,
                EnableBatchedOperations = true,
                LockDuration = TimeSpan.FromSeconds(30),
                RequiresSession = false,
                AutoDeleteOnIdle = TimeSpan.FromDays(367),
            };
            if (_namespaceManager.SubscriptionExists(topicPath, subscriptionName))
            {
                _namespaceManager.UpdateSubscription(subscriptionDescription);
            }
            else
            {
                _namespaceManager.CreateSubscription(subscriptionDescription);
            }
        }

        public void EnsureTopicExists(Type eventType)
        {
            var topicPath = PathFactory.TopicPathFor(eventType);
            EnsureTopicExists(topicPath);
        }

        private void EnsureTopicExists(string topicPath)
        {
            _logger.Debug("Ensuring topic '{0}' exists", topicPath);

            var topicDescription = new TopicDescription(topicPath)
            {
                DefaultMessageTimeToLive = TimeSpan.MaxValue,
                EnableBatchedOperations = true,
                RequiresDuplicateDetection = false,
                SupportOrdering = false,
                AutoDeleteOnIdle = TimeSpan.FromDays(367),
            };

            if (_namespaceManager.TopicExists(topicPath))
            {
                _namespaceManager.UpdateTopic(topicDescription);
            }
            else
            {
                _namespaceManager.CreateTopic(topicDescription);
            }
        }

        public void EnsureQueueExists(Type commandType)
        {
            var queuePath = PathFactory.QueuePathFor(commandType);
            EnsureQueueExists(queuePath);
        }

        public void EnsureQueueExists(string queuePath)
        {
            _logger.Debug("Ensuring queue '{0}' exists", queuePath);

            var queueDescription = new QueueDescription(queuePath)
            {
                MaxDeliveryCount = _maxDeliveryAttempts,
                DefaultMessageTimeToLive = TimeSpan.MaxValue,
                EnableDeadLetteringOnMessageExpiration = true,
                EnableBatchedOperations = true,
                LockDuration = TimeSpan.FromSeconds(30),
                RequiresDuplicateDetection = false,
                RequiresSession = false,
                SupportOrdering = false,
                AutoDeleteOnIdle = TimeSpan.FromDays(367),
            };

            if (_namespaceManager.QueueExists(queuePath))
            {
                _namespaceManager.UpdateQueue(queueDescription);
            }
            else
            {
                _namespaceManager.CreateQueue(queueDescription);
            }
        }

        private string GetDeadLetterQueueName(Type messageContractType)
        {
            var queuePath = PathFactory.QueuePathFor(messageContractType);
            var deadLetterQueueName = QueueClient.FormatDeadLetterPath(queuePath);
            return deadLetterQueueName;
        }

        public QueueClient CreateQueueClient<T>()
        {
            var messageContractType = typeof (T);
            var queueName = PathFactory.QueuePathFor(messageContractType);

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