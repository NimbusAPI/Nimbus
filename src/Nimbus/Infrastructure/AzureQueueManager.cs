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
        private readonly Func<NamespaceManager> _namespaceManager;
        private readonly Func<MessagingFactory> _messagingFactory;
        private readonly MaxDeliveryAttemptSetting _maxDeliveryAttempts;
        private readonly ILogger _logger;

        private readonly Lazy<ConcurrentBag<string>> _knownTopics;
        private readonly Lazy<ConcurrentBag<string>> _knownSubscriptions;
        private readonly Lazy<ConcurrentBag<string>> _knownQueues;

        public AzureQueueManager(Func<NamespaceManager> namespaceManager,
                                 Func<MessagingFactory> messagingFactory,
                                 MaxDeliveryAttemptSetting maxDeliveryAttempts,
                                 ILogger logger)
        {
            _namespaceManager = namespaceManager;
            _messagingFactory = messagingFactory;
            _maxDeliveryAttempts = maxDeliveryAttempts;
            _logger = logger;

            _knownTopics = new Lazy<ConcurrentBag<string>>(FetchExistingTopics);
            _knownSubscriptions = new Lazy<ConcurrentBag<string>>(FetchExistingSubscriptions);
            _knownQueues = new Lazy<ConcurrentBag<string>>(FetchExistingQueues);
        }

        public MessageSender CreateMessageSender(string queuePath)
        {
            EnsureQueueExists(queuePath);
            return _messagingFactory().CreateMessageSender(queuePath);
        }

        public MessageReceiver CreateMessageReceiver(string queuePath)
        {
            EnsureQueueExists(queuePath);
            return _messagingFactory().CreateMessageReceiver(queuePath);
        }

        public TopicClient CreateTopicSender(string topicPath)
        {
            EnsureTopicExists(topicPath);
            return _messagingFactory().CreateTopicClient(topicPath);
        }

        public SubscriptionClient CreateSubscriptionReceiver(string topicPath, string subscriptionName)
        {
            EnsureSubscriptionExists(topicPath, subscriptionName);
            return _messagingFactory().CreateSubscriptionClient(topicPath, subscriptionName);
        }

        public QueueClient CreateQueueClient<T>()
        {
            var messageContractType = typeof (T);
            var queueName = PathFactory.QueuePathFor(messageContractType);

            EnsureQueueExists(messageContractType);
            return _messagingFactory().CreateQueueClient(queueName);
        }

        public QueueClient CreateDeadLetterQueueClient<T>()
        {
            var messageContractType = typeof (T);
            var queueName = GetDeadLetterQueueName(messageContractType);

            EnsureQueueExists(messageContractType);
            return _messagingFactory().CreateQueueClient(queueName);
        }

        private ConcurrentBag<string> FetchExistingTopics()
        {
            _logger.Debug("Fetching existing topics...");

            var topics = _namespaceManager().GetTopics();
            return new ConcurrentBag<string>(topics.Select(t => t.Path));
        }

        private ConcurrentBag<string> FetchExistingSubscriptions()
        {
            _logger.Debug("Fetching existing subscriptions...");

            var subscriptionKeys = from topicPath in _knownTopics.Value.AsParallel()
                                   from subscriptionName in _namespaceManager().GetSubscriptions(topicPath).Select(s => s.Name)
                                   select BuildSubscriptionKey(topicPath, subscriptionName);

            return new ConcurrentBag<string>(subscriptionKeys);
        }

        private ConcurrentBag<string> FetchExistingQueues()
        {
            _logger.Debug("Fetching existing queues...");

            var queuesAsync = _namespaceManager().GetQueuesAsync();
            if (!queuesAsync.Wait(TimeSpan.FromSeconds(5))) throw new TimeoutException("Fetching existing queues failed. Messaging endpoint did not respond in time.");

            var queues = queuesAsync.Result;
            return new ConcurrentBag<string>(queues.Select(q => q.Path));
        }

        private void EnsureTopicExists(string topicPath)
        {
            if (_knownTopics.Value.Contains(topicPath)) return;

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
                _namespaceManager().CreateTopic(topicDescription);
            }
            catch (MessagingEntityAlreadyExistsException)
            {
            }
            catch (MessagingException exc)
            {
                if (!exc.Message.Contains("SubCode=40901")) throw;

                // SubCode=40901. Another conflicting operation is in progress. Let's see if it's created the topic for us.
                if (!_namespaceManager().TopicExists(topicPath)) throw new BusException("Topic creation for '{0}' failed".FormatWith(topicPath));
            }

            _knownTopics.Value.Add(topicPath);
        }

        private void EnsureSubscriptionExists(string topicPath, string subscriptionName)
        {
            var subscriptionKey = BuildSubscriptionKey(topicPath, subscriptionName);
            if (_knownSubscriptions.Value.Contains(subscriptionKey)) return;

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
                _namespaceManager().CreateSubscription(subscriptionDescription);
            }
            catch (MessagingEntityAlreadyExistsException)
            {
            }
            catch (MessagingException exc)
            {
                if (!exc.Message.Contains("SubCode=40901")) throw;

                // SubCode=40901. Another conflicting operation is in progress. Let's see if it's created the subscription for us.
                if (!_namespaceManager().SubscriptionExists(topicPath, subscriptionName))
                    throw new BusException("Subscription creation for '{0}/{1}' failed".FormatWith(topicPath, subscriptionName));
            }

            _knownSubscriptions.Value.Add(subscriptionKey);
        }

        private static string BuildSubscriptionKey(string topicPath, string subscriptionName)
        {
            return "{0}/{1}".FormatWith(topicPath, subscriptionName);
        }

        private void EnsureQueueExists(Type commandType)
        {
            var queuePath = PathFactory.QueuePathFor(commandType);
            EnsureQueueExists(queuePath);
        }

        private void EnsureQueueExists(string queuePath)
        {
            if (_knownQueues.Value.Contains(queuePath)) return;

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
                _namespaceManager().CreateQueue(queueDescription);
            }
            catch (MessagingEntityAlreadyExistsException exc)
            {
            }
            catch (MessagingException exc)
            {
                if (!exc.Message.Contains("SubCode=40901")) throw;

                // SubCode=40901. Another conflicting operation is in progress. Let's see if it's created the queue for us.
                if (!_namespaceManager().QueueExists(queuePath)) throw new BusException("Queue creation for '{0}' failed".FormatWith(queuePath));
            }

            _knownQueues.Value.Add(queuePath);
        }

        private string GetDeadLetterQueueName(Type messageContractType)
        {
            var queuePath = PathFactory.QueuePathFor(messageContractType);
            var deadLetterQueueName = QueueClient.FormatDeadLetterPath(queuePath);
            return deadLetterQueueName;
        }
    }
}