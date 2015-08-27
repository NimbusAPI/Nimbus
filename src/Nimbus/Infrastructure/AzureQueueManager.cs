using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Nimbus.ConcurrentCollections;
using Nimbus.Configuration.Settings;
using Nimbus.Extensions;
using Nimbus.MessageContracts.Exceptions;
using Nimbus.Routing;

namespace Nimbus.Infrastructure
{
    internal class AzureQueueManager : IQueueManager
    {
        private readonly Func<NamespaceManager> _namespaceManager;
        private readonly Func<MessagingFactory> _messagingFactory;
        private readonly MaxDeliveryAttemptSetting _maxDeliveryAttempts;
        private readonly DefaultMessageTimeToLiveSetting _defaultMessageTimeToLive;
        private readonly AutoDeleteOnIdleSetting _autoDeleteOnIdle;
        private readonly AutoRecreateMessagingEntitySetting _autoRecreateMessagingEntitySetting;
        private readonly EnableDeadLetteringOnMessageExpirationSetting _enableDeadLetteringOnMessageExpiration;
        private readonly ILogger _logger;
        private readonly IRouter _router;

        private readonly ThreadSafeLazy<ConcurrentDictionary<string, object>> _knownTopics;
        private readonly ThreadSafeLazy<ConcurrentDictionary<string, object>> _knownSubscriptions;
        private readonly ThreadSafeLazy<ConcurrentDictionary<string, object>> _knownQueues;
        private readonly DefaultMessageLockDurationSetting _defaultMessageLockDuration;
        private readonly ITypeProvider _typeProvider;

        private readonly ThreadSafeDictionary<string, object> _locks = new ThreadSafeDictionary<string, object>();

        public AzureQueueManager(Func<NamespaceManager> namespaceManager,
                                 Func<MessagingFactory> messagingFactory,
                                 MaxDeliveryAttemptSetting maxDeliveryAttempts,
                                 ILogger logger,
                                 IRouter router,
                                 DefaultMessageLockDurationSetting defaultMessageLockDuration,
                                 ITypeProvider typeProvider,
                                 DefaultMessageTimeToLiveSetting defaultMessageTimeToLive,
                                 AutoDeleteOnIdleSetting autoDeleteOnIdle,
                                 AutoRecreateMessagingEntitySetting autoRecreateMessagingEntitySetting,
                                 EnableDeadLetteringOnMessageExpirationSetting enableDeadLetteringOnMessageExpiration)
        {
            _namespaceManager = namespaceManager;
            _messagingFactory = messagingFactory;
            _maxDeliveryAttempts = maxDeliveryAttempts;
            _logger = logger;
            _router = router;
            _defaultMessageLockDuration = defaultMessageLockDuration;
            _typeProvider = typeProvider;
            _defaultMessageTimeToLive = defaultMessageTimeToLive;
            _autoDeleteOnIdle = autoDeleteOnIdle;
            _autoRecreateMessagingEntitySetting = autoRecreateMessagingEntitySetting;
            _enableDeadLetteringOnMessageExpiration = enableDeadLetteringOnMessageExpiration;

            _knownTopics = new ThreadSafeLazy<ConcurrentDictionary<string, object>>(FetchExistingTopics);
            _knownSubscriptions = new ThreadSafeLazy<ConcurrentDictionary<string, object>>(FetchExistingSubscriptions);
            _knownQueues = new ThreadSafeLazy<ConcurrentDictionary<string, object>>(FetchExistingQueues);
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

        public Task<MessageSender> CreateMessageSender(string queuePath)
        {
            EnsureQueueExists(queuePath);
            return _messagingFactory().CreateMessageSenderAsync(queuePath);
        }

        public Task<MessageReceiver> CreateMessageReceiver(string queuePath)
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

        private ConcurrentDictionary<string, object> FetchExistingTopics()
        {
            _logger.Debug("Fetching existing topics...");

            var topicsAsync = _namespaceManager().GetTopicsAsync();
            if (!topicsAsync.Wait(TimeSpan.FromSeconds(10))) throw new TimeoutException("Fetching existing topics failed. Messaging endpoint did not respond in time.");

            var topics = topicsAsync.Result;
            var topicPaths = new ConcurrentDictionary<string, object>(topics.Select(t => new KeyValuePair<string, object>(t.Path,null)));

            return topicPaths;
        }

        private ConcurrentDictionary<string, object> FetchExistingSubscriptions()
        {
            _logger.Debug("Fetching existing subscriptions...");

            var subscriptionTasks = _knownTopics.Value
                                                .Where(x => WeHaveAHandler(x.Key))
                                                .Select(x => FetchExistingTopicSubscriptions(x.Key))
                                                .ToArray();

            Task.WaitAll(subscriptionTasks.Cast<Task>().ToArray());

            var subscriptionKeys = subscriptionTasks
                .SelectMany(t => t.Result)
                .OrderBy(k => k)
                .ToArray();

            return new ConcurrentDictionary<string, object>(subscriptionKeys.Select(t => new KeyValuePair<string, object>(t, null)));
        }

        private bool WeHaveAHandler(string topicPath)
        {
            var paths = _typeProvider.AllTypesHandledViaTopics().Select(PathFactory.TopicPathFor);
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

        private ConcurrentDictionary<string, object> FetchExistingQueues()
        {
            _logger.Debug("Fetching existing queues...");

            var queuesAsync = _namespaceManager().GetQueuesAsync();
            if (!queuesAsync.Wait(TimeSpan.FromSeconds(10))) throw new TimeoutException("Fetching existing queues failed. Messaging endpoint did not respond in time.");

            var queues = queuesAsync.Result;
            var queuePaths = queues.Select(q => q.Path)
                                   .OrderBy(p => p)
                                   .ToArray();
            return new ConcurrentDictionary<string, object>(queuePaths.Select(s => new KeyValuePair<string, object>(s, null)));
        }

        private void EnsureTopicExists(string topicPath)
        {
            if (_knownTopics.Value.ContainsKey(topicPath)) return;
            lock (LockFor(topicPath))
            {
                if (_knownTopics.Value.ContainsKey(topicPath)) return;

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

                _knownTopics.Value.SafeAddKey(topicPath);
            }
        }

        public void RemoveSubscription(string topicPath, string subscriptionName)
        {
            if (_autoRecreateMessagingEntitySetting.Value)
            {
                if (_knownTopics.Value.ContainsKey(topicPath))
                    _knownTopics.Value.SafeRemoveKey(topicPath);

                var subscriptionKey = BuildSubscriptionKey(topicPath, subscriptionName);
                if (_knownSubscriptions.Value.ContainsKey(subscriptionKey))
                    _knownSubscriptions.Value.SafeRemoveKey(subscriptionKey);

            }
        }

        private void EnsureSubscriptionExists(string topicPath, string subscriptionName)
        {
            var subscriptionKey = BuildSubscriptionKey(topicPath, subscriptionName);

            if (_knownSubscriptions.Value.ContainsKey(subscriptionKey)) return;
            lock (LockFor(subscriptionKey))
            {
                if (_knownSubscriptions.Value.ContainsKey(subscriptionKey)) return;

                EnsureTopicExists(topicPath);

                _logger.Debug("Creating subscription '{0}'", subscriptionKey);

                var subscriptionDescription = new SubscriptionDescription(topicPath, subscriptionName)
                                              {
                                                  MaxDeliveryCount = _maxDeliveryAttempts,
                                                  DefaultMessageTimeToLive = _defaultMessageTimeToLive,
                                                  EnableDeadLetteringOnMessageExpiration = _enableDeadLetteringOnMessageExpiration,
                                                  EnableBatchedOperations = true,
                                                  LockDuration = _defaultMessageLockDuration,
                                                  RequiresSession = false,
                                                  AutoDeleteOnIdle = _autoDeleteOnIdle,
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

                _knownSubscriptions.Value.SafeAddKey(subscriptionKey);
            }
        }

        private static string BuildSubscriptionKey(string topicPath, string subscriptionName)
        {
            return "{0}/{1}".FormatWith(topicPath, subscriptionName);
        }

        private void EnsureQueueExists(Type commandType)
        {
            var queuePath = _router.Route(commandType, QueueOrTopic.Queue);
            EnsureQueueExists(queuePath);
        }

        internal void EnsureQueueExists(string queuePath)
        {
            if (_knownQueues.Value.ContainsKey(queuePath)) return;

            lock (LockFor(queuePath))
            {
                if (_knownQueues.Value.ContainsKey(queuePath)) return;

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
                                           AutoDeleteOnIdle = _autoDeleteOnIdle,
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
                    if (!_namespaceManager().QueueExists(queuePath)) throw new BusException("Queue creation for '{0}' failed".FormatWith(queuePath), exc);
                }

                _knownQueues.Value.SafeAddKey(queuePath);
            }
        }

        public void RemoveQueue(string queuePath)
        {
            if (_autoRecreateMessagingEntitySetting.Value)
            {
                if (_knownQueues.Value.ContainsKey(queuePath))
                    _knownQueues.Value.SafeRemoveKey(queuePath);
            }
        }

        private string GetDeadLetterQueueName(Type messageContractType)
        {
            var queuePath = _router.Route(messageContractType, QueueOrTopic.Queue);
            var deadLetterQueueName = QueueClient.FormatDeadLetterPath(queuePath);
            return deadLetterQueueName;
        }

        private object LockFor(string path)
        {
            return _locks.GetOrAdd(path, p => new object());
        }
    }
}
