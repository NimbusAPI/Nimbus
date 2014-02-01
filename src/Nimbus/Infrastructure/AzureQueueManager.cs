using System;
using System.Collections.Concurrent;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Nimbus.Configuration.Settings;
using Nimbus.Extensions;
using Nimbus.InfrastructureContracts;
using Nimbus.MessageContracts.Exceptions;

namespace Nimbus.Infrastructure
{
    public class AzureQueueManager : IQueueManager
    {
        private readonly NamespaceManager _namespaceManager;
        private readonly MessagingFactory _messagingFactory;
        private readonly MaxDeliveryAttemptSetting _maxDeliveryAttempts;
        private readonly ILogger _logger;

        private readonly ConcurrentDictionary<Type, QueueClient> _queueClients = new ConcurrentDictionary<Type, QueueClient>();
        private readonly ConcurrentDictionary<Type, QueueClient> _deadLetterQueueClients = new ConcurrentDictionary<Type, QueueClient>();

        public AzureQueueManager(NamespaceManager namespaceManager, MessagingFactory messagingFactory, MaxDeliveryAttemptSetting maxDeliveryAttempts, ILogger logger)
        {
            _namespaceManager = namespaceManager;
            _messagingFactory = messagingFactory;
            _maxDeliveryAttempts = maxDeliveryAttempts;
            _logger = logger;
        }

        public void EnsureSubscriptionExists(Type eventType, string subscriptionName)
        {
            EnsureSubscriptionExists(PathFactory.TopicPathFor(eventType), subscriptionName);
        }

        private void EnsureSubscriptionExists(string topicPath, string subscriptionName)
        {
            EnsureTopicExists(topicPath);
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

            try
            {
                _namespaceManager.CreateSubscription(subscriptionDescription);
            }
            catch (MessagingEntityAlreadyExistsException)
            {
                _logger.Debug("Subscription '{0}' for topic '{1}' has already been created.", subscriptionName, topicPath);
                _namespaceManager.UpdateSubscription(subscriptionDescription);
            }

            _logger.Debug("Updating subscription '{0}' for topic '{1}'.", subscriptionName, topicPath);
            _namespaceManager.UpdateSubscription(subscriptionDescription);
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

            // We don't check for topic existence here because that introduces a race condition with any other bus participant that's
            // launching at the same time. If it doesn't exist, we'll create it. If it does, we'll just continue on with life and
            // update its configuration in a minute anyway.  -andrewh 8/12/2013
            try
            {
                _namespaceManager.CreateTopic(topicDescription);
            }
            catch (MessagingException)
            {
            }
            try
            {
                _namespaceManager.UpdateTopic(topicDescription);
            }
            catch (MessagingException)
            {
            }

            if (!_namespaceManager.TopicExists(topicPath))
            {
                throw new BusException("Topic creation for '{0}' failed".FormatWith(topicPath));
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

            // We don't check for queue existence here because that introduces a race condition with any other bus participant that's
            // launching at the same time. If it doesn't exist, we'll create it. If it does, we'll just continue on with life and
            // update its configuration in a minute anyway.  -andrewh 8/12/2013
            try
            {
                _namespaceManager.CreateQueue(queueDescription);
            }
            catch (MessagingException)
            {
            }
            try
            {
                _namespaceManager.UpdateQueue(queueDescription);
            }
            catch (MessagingException)
            {
            }

            if (!_namespaceManager.QueueExists(queuePath))
            {
                throw new BusException("Queue creation for '{0}' failed".FormatWith(queuePath));
            }
        }

        public MessageSender CreateMessageSender(Type messageType)
        {
            var queuePath = PathFactory.QueuePathFor(messageType);
            return CreateMessageSender(queuePath);
        }

        public MessageSender CreateMessageSender(string queuePath)
        {
            EnsureQueueExists(queuePath);
            return _messagingFactory.CreateMessageSender(queuePath);
        }

        public MessageReceiver CreateMessageReceiver(string queuePath)
        {
            EnsureQueueExists(queuePath);
            return _messagingFactory.CreateMessageReceiver(queuePath);
        }

        public SubscriptionClient CreateSubscriptionReceiver(string topicPath, string subscriptionName)
        {
            EnsureSubscriptionExists(topicPath, subscriptionName);
            return _messagingFactory.CreateSubscriptionClient(topicPath, subscriptionName);
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