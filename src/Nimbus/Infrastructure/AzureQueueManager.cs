using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Nimbus.ConcurrentCollections;
using Nimbus.Configuration.Settings;
using Nimbus.Extensions;
using Nimbus.MessageContracts.Exceptions;

namespace Nimbus.Infrastructure
{
    internal class AzureQueueManager : IQueueManager
    {
        private readonly Func<NamespaceManager> _namespaceManager;
        private readonly Func<MessagingFactory> _messagingFactory;
        private readonly MaxDeliveryAttemptSetting _maxDeliveryAttempts;
        private readonly ILogger _logger;

        private readonly ThreadSafeLazy<ConcurrentBag<string>> _knownTopics;
        private readonly ThreadSafeLazy<ConcurrentBag<string>> _knownSubscriptions;
        private readonly ThreadSafeLazy<ConcurrentBag<string>> _knownQueues;
        private readonly DefaultMessageLockDurationSetting _defaultMessageLockDuration;
        private readonly ITypeProvider _typeProvider;

        private readonly ThreadSafeDictionary<string, object> _locks = new ThreadSafeDictionary<string, object>(); 

        public AzureQueueManager(Func<NamespaceManager> namespaceManager,
                                 Func<MessagingFactory> messagingFactory,
                                 MaxDeliveryAttemptSetting maxDeliveryAttempts,
                                 ILogger logger,
                                 DefaultMessageLockDurationSetting defaultMessageLockDuration,
            ITypeProvider typeProvider)
        {
            _namespaceManager = namespaceManager;
            _messagingFactory = messagingFactory;
            _maxDeliveryAttempts = maxDeliveryAttempts;
            _logger = logger;
            _defaultMessageLockDuration = defaultMessageLockDuration;
            _typeProvider = typeProvider;

            _knownTopics = new ThreadSafeLazy<ConcurrentBag<string>>(FetchExistingTopics);
            _knownSubscriptions = new ThreadSafeLazy<ConcurrentBag<string>>(FetchExistingSubscriptions);
            _knownQueues = new ThreadSafeLazy<ConcurrentBag<string>>(FetchExistingQueues);
        }

        public void WarmUp()
        {
            try
            {
                // ReSharper disable UnusedVariable
                var task0 = Task.Run(() => { var dummy0 = _knownQueues.Value; });
                var task1 = Task.Run(() => { var dummy1 = _knownSubscriptions.Value; });
                // ReSharper restore UnusedVariable

                Task.WaitAll(task0, task1);
            }
            catch (Exception exc)
            {
                throw new BusException("Azure queue manager failed to start", exc);
            }
        }

        public  Task<MessageSender> CreateMessageSender(string queuePath)
        {
            EnsureQueueExists(queuePath);
            return  _messagingFactory().CreateMessageSenderAsync(queuePath);
        }

        public  Task<MessageReceiver> CreateMessageReceiver(string queuePath)
        {
            EnsureQueueExists(queuePath);
            return _messagingFactory().CreateMessageReceiverAsync(queuePath);
        }

        public Task<TopicClient> CreateTopicSender(string topicPath)
        {
            return Task.Run(() =>
            {
                EnsureTopicExists(topicPath);
                return _messagingFactory().CreateTopicClient(topicPath);
            });
        }

        public Task<SubscriptionClient> CreateSubscriptionReceiver(string topicPath, string subscriptionName)
        {
            return Task.Run(() =>
            {
                EnsureSubscriptionExists(topicPath, subscriptionName);
                return _messagingFactory().CreateSubscriptionClient(topicPath, subscriptionName);
            });
        }

        public QueueClient CreateQueueClient<T>()
        {
            var messageContractType = typeof(T);
            var queueName = PathFactory.QueuePathFor(messageContractType);

            EnsureQueueExists(messageContractType);
            return _messagingFactory().CreateQueueClient(queueName);
        }

        public Task<QueueClient> CreateDeadLetterQueueClient<T>()
        {
            return Task.Run(() =>
            {
                var messageContractType = typeof(T);
                var queueName = GetDeadLetterQueueName(messageContractType);

                EnsureQueueExists(messageContractType);
                return _messagingFactory().CreateQueueClient(queueName);
            });
        }

        private ConcurrentBag<string> FetchExistingTopics()
        {
            _logger.Debug("Fetching existing topics...");

            var topicsAsync = _namespaceManager().GetTopicsAsync();
            if (!topicsAsync.Wait(TimeSpan.FromSeconds(10))) throw new TimeoutException("Fetching existing topics failed. Messaging endpoint did not respond in time.");

            var topics = topicsAsync.Result;
            var topicPaths = new ConcurrentBag<string>(topics.Select(t => t.Path));

            return topicPaths;
        }

        private ConcurrentBag<string> FetchExistingSubscriptions()
        {
            _logger.Debug("Fetching existing subscriptions...");

            var subscriptionTasks = _knownTopics.Value
                                                .Where(WeHaveAHandler)
                                                .Select(FetchExistingTopicSubscriptions)
                                                .ToArray();

            Task.WaitAll(subscriptionTasks.Cast<Task>().ToArray());

            var subscriptionKeys = subscriptionTasks
                .SelectMany(t => t.Result)
                .OrderBy(k => k)
                .ToArray();

            return new ConcurrentBag<string>(subscriptionKeys);
        }

        private bool WeHaveAHandler(string topicPath)
        {
            var paths = _typeProvider.AllHandledEventTypes().Select(PathFactory.TopicPathFor);
            return paths.Contains(topicPath);
        }

        private Task<string[]> FetchExistingTopicSubscriptions(string topicPath)
        {
            return Task.Run(async () =>
                                  {
                                      var subscriptions = await _namespaceManager().GetSubscriptionsAsync(topicPath);

                                      return subscriptions
                                          .Select(s => s.Name)
                                          .Select(subscriptionName => BuildSubscriptionKey(topicPath, subscriptionName))
                                          .ToArray();
                                  });
        }

        private ConcurrentBag<string> FetchExistingQueues()
        {
            _logger.Debug("Fetching existing queues...");

            var queuesAsync = _namespaceManager().GetQueuesAsync();
            if (!queuesAsync.Wait(TimeSpan.FromSeconds(10))) throw new TimeoutException("Fetching existing queues failed. Messaging endpoint did not respond in time.");

            var queues = queuesAsync.Result;
            var queuePaths = queues.Select(q => q.Path)
                                   .OrderBy(p => p)
                                   .ToArray();
            return new ConcurrentBag<string>(queuePaths);
        }

        private void EnsureTopicExists(string topicPath)
        {
            if (_knownTopics.Value.Contains(topicPath)) return;
            lock (LockFor(topicPath))
            {
                if (_knownTopics.Value.Contains(topicPath)) return;

                _logger.Debug("Creating topic '{0}'", topicPath);

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
        }

        private void EnsureSubscriptionExists(string topicPath, string subscriptionName)
        {
            var subscriptionKey = BuildSubscriptionKey(topicPath, subscriptionName);

            if (_knownSubscriptions.Value.Contains(subscriptionKey)) return;
            lock (LockFor(subscriptionKey))
            {
                if (_knownSubscriptions.Value.Contains(subscriptionKey)) return;

                EnsureTopicExists(topicPath);

                _logger.Debug("Creating subscription '{0}'", subscriptionKey);

                var subscriptionDescription = new SubscriptionDescription(topicPath, subscriptionName)
                                              {
                                                  MaxDeliveryCount = _maxDeliveryAttempts,
                                                  DefaultMessageTimeToLive = TimeSpan.MaxValue,
                                                  EnableDeadLetteringOnMessageExpiration = true,
                                                  EnableBatchedOperations = true,
                                                  LockDuration = _defaultMessageLockDuration,
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

        internal void EnsureQueueExists(string queuePath)
        {
            if (_knownQueues.Value.Contains(queuePath)) return;

            lock (LockFor(queuePath))
            {
                if (_knownQueues.Value.Contains(queuePath)) return;

                _logger.Debug("Creating queue '{0}'", queuePath);

                var queueDescription = new QueueDescription(queuePath)
                                       {
                                           MaxDeliveryCount = _maxDeliveryAttempts,
                                           DefaultMessageTimeToLive = TimeSpan.MaxValue,
                                           EnableDeadLetteringOnMessageExpiration = true,
                                           EnableBatchedOperations = true,
                                           LockDuration = _defaultMessageLockDuration,
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
                catch (MessagingEntityAlreadyExistsException)
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
        }

        private string GetDeadLetterQueueName(Type messageContractType)
        {
            var queuePath = PathFactory.QueuePathFor(messageContractType);
            var deadLetterQueueName = QueueClient.FormatDeadLetterPath(queuePath);
            return deadLetterQueueName;
        }

        private object LockFor(string path)
        {
            return _locks.GetOrAdd(path, p => new object());
        }
    }
}