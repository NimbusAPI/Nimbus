using System;
using System.Collections.Concurrent;
using System.Linq;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Nimbus.Configuration.Settings;
using Nimbus.Extensions;
using Nimbus.InfrastructureContracts;
using Nimbus.MessageContracts.Exceptions;

namespace Nimbus.Infrastructure
{
    internal class AzureQueueManager : IQueueManager
    {
        private readonly NamespaceManager _namespaceManager;
        private readonly MessagingFactory _messagingFactory;
        private readonly MaxDeliveryAttemptSetting _maxDeliveryAttempts;
        private readonly ILogger _logger;

        private readonly ConcurrentBag<string> _knownTopics = new ConcurrentBag<string>();
        private readonly ConcurrentBag<string> _knownSubscriptions = new ConcurrentBag<string>();
        private readonly ConcurrentBag<string> _knownQueues = new ConcurrentBag<string>();

        public AzureQueueManager(NamespaceManager namespaceManager, MessagingFactory messagingFactory, MaxDeliveryAttemptSetting maxDeliveryAttempts, ILogger logger)
        {
            _namespaceManager = namespaceManager;
            _messagingFactory = messagingFactory;
            _maxDeliveryAttempts = maxDeliveryAttempts;
            _logger = logger;
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

        public TopicClient CreateTopicSender(string topicPath)
        {
            EnsureTopicExists(topicPath);
            return _messagingFactory.CreateTopicClient(topicPath);
        }

        public SubscriptionClient CreateSubscriptionReceiver(string topicPath, string subscriptionName)
        {
            EnsureSubscriptionExists(topicPath, subscriptionName);
            return _messagingFactory.CreateSubscriptionClient(topicPath, subscriptionName);
        }

        public QueueClient CreateQueueClient<T>()
        {
            var messageContractType = typeof (T);
            var queueName = PathFactory.QueuePathFor(messageContractType);

            EnsureQueueExists(messageContractType);
            return _messagingFactory.CreateQueueClient(queueName);
        }

        public QueueClient CreateDeadLetterQueueClient<T>()
        {
            var messageContractType = typeof (T);
            var queueName = GetDeadLetterQueueName(messageContractType);

            EnsureQueueExists(messageContractType);
            return _messagingFactory.CreateQueueClient(queueName);
        }

        private void EnsureTopicExists(string topicPath)
        {
            if (_knownTopics.Contains(topicPath)) return;

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
            catch (MessagingEntityAlreadyExistsException)
            {
            }
            catch (MessagingException exc)
            {
                if (exc.Message.Contains("SubCode=40901"))
                {
                    // SubCode=40901. Another conflicting operation is in progress. Ignore.
                }
                else
                {
                    throw;
                }
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

            _knownTopics.Add(topicPath);
        }

        private void EnsureSubscriptionExists(string topicPath, string subscriptionName)
        {
            var subscriptionKey = "{0}/{1}".FormatWith(topicPath, subscriptionName);
            if (_knownSubscriptions.Contains(subscriptionKey)) return;

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
            catch (MessagingException)
            {
            }

            try
            {
                _namespaceManager.UpdateSubscription(subscriptionDescription);
            }
            catch (MessagingException)
            {
            }

            _logger.Debug("Updating subscription '{0}'.", subscriptionKey);
            _namespaceManager.UpdateSubscription(subscriptionDescription);

            if (!_namespaceManager.SubscriptionExists(topicPath, subscriptionName))
            {
                throw new BusException("Subscription creation for '{0}/{1}' failed".FormatWith(topicPath, subscriptionName));
            }

            _knownSubscriptions.Add(subscriptionKey);
        }

        private void EnsureQueueExists(Type commandType)
        {
            var queuePath = PathFactory.QueuePathFor(commandType);
            EnsureQueueExists(queuePath);
        }

        private void EnsureQueueExists(string queuePath)
        {
            if (_knownQueues.Contains(queuePath)) return;

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
            catch (MessagingEntityAlreadyExistsException)
            {
            }
            catch (MessagingException exc)
            {
                if (exc.Message.Contains("SubCode=40901"))
                {
                    // SubCode=40901. Another conflicting operation is in progress. Ignore.
                }
                else
                {
                    throw;
                }
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
            _knownQueues.Add(queuePath);
        }

        private string GetDeadLetterQueueName(Type messageContractType)
        {
            var queuePath = PathFactory.QueuePathFor(messageContractType);
            var deadLetterQueueName = QueueClient.FormatDeadLetterPath(queuePath);
            return deadLetterQueueName;
        }
    }
}