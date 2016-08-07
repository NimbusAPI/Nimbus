using System;
using System.Threading.Tasks;
using Microsoft.ServiceBus;
using Nimbus.ConcurrentCollections;
using Nimbus.Configuration.PoorMansIocContainer;
using Nimbus.Configuration.Settings;
using Nimbus.DependencyResolution;
using Nimbus.Extensions;
using Nimbus.Filtering.Conditions;
using Nimbus.Infrastructure;
using Nimbus.Infrastructure.MessageSendersAndReceivers;
using Nimbus.Infrastructure.Retries;
using Nimbus.Transports.AzureServiceBus.BrokeredMessages;
using Nimbus.Transports.AzureServiceBus.Filtering;
using Nimbus.Transports.AzureServiceBus.QueueManagement;
using Nimbus.Transports.AzureServiceBus.SendersAndRecievers;

namespace Nimbus.Transports.AzureServiceBus
{
    internal class AzureServiceBusTransport : INimbusTransport, IDisposable
    {
        private readonly IQueueManager _queueManager;
        private readonly Func<NamespaceManager> _namespaceManager;
        private readonly ConcurrentHandlerLimitSetting _concurrentHandlerLimit;
        private readonly IBrokeredMessageFactory _brokeredMessageFactory;
        private readonly IGlobalHandlerThrottle _globalHandlerThrottle;
        private readonly ILogger _logger;

        private readonly ThreadSafeDictionary<string, INimbusMessageSender> _queueMessageSenders = new ThreadSafeDictionary<string, INimbusMessageSender>();
        private readonly ThreadSafeDictionary<string, INimbusMessageReceiver> _queueMessageReceivers = new ThreadSafeDictionary<string, INimbusMessageReceiver>();
        private readonly ThreadSafeDictionary<string, INimbusMessageSender> _topicMessageSenders = new ThreadSafeDictionary<string, INimbusMessageSender>();
        private readonly ThreadSafeDictionary<string, INimbusMessageReceiver> _topicMessageReceivers = new ThreadSafeDictionary<string, INimbusMessageReceiver>();
        private readonly GarbageMan _garbageMan = new GarbageMan();
        private readonly IRetry _retry;
        private readonly IDependencyResolver _dependencyResolver;
        private readonly ISqlFilterExpressionGenerator _sqlFilterExpressionGenerator;

        public AzureServiceBusTransport(ConcurrentHandlerLimitSetting concurrentHandlerLimit,
                                        IBrokeredMessageFactory brokeredMessageFactory,
                                        IGlobalHandlerThrottle globalHandlerThrottle,
                                        ILogger logger,
                                        IQueueManager queueManager,
                                        Func<NamespaceManager> namespaceManager,
                                        IRetry retry,
                                        IDependencyResolver dependencyResolver,
                                        ISqlFilterExpressionGenerator sqlFilterExpressionGenerator)
        {
            _queueManager = queueManager;
            _namespaceManager = namespaceManager;
            _retry = retry;
            _dependencyResolver = dependencyResolver;
            _sqlFilterExpressionGenerator = sqlFilterExpressionGenerator;
            _brokeredMessageFactory = brokeredMessageFactory;
            _globalHandlerThrottle = globalHandlerThrottle;
            _concurrentHandlerLimit = concurrentHandlerLimit;
            _logger = logger;
        }

        public async Task TestConnection()
        {
            var version = await _namespaceManager().GetVersionInfoAsync();
            _logger.Debug("Azure Service Bus transport is online with API version {ApiVersion}", version);
        }

        public INimbusMessageSender GetQueueSender(string queuePath)
        {
            return _queueMessageSenders.GetOrAdd(queuePath, CreateQueueSender);
        }

        public INimbusMessageReceiver GetQueueReceiver(string queuePath)
        {
            return _queueMessageReceivers.GetOrAdd(queuePath, CreateQueueReceiver);
        }

        public INimbusMessageSender GetTopicSender(string topicPath)
        {
            return _topicMessageSenders.GetOrAdd(topicPath, CreateTopicSender);
        }

        public INimbusMessageReceiver GetTopicReceiver(string topicPath, string subscriptionName, IFilterCondition filter)
        {
            var key = "{0}/{1}".FormatWith(topicPath, subscriptionName);
            return _topicMessageReceivers.GetOrAdd(key, k => CreateTopicReceiver(topicPath, subscriptionName, filter));
        }

        private INimbusMessageSender CreateQueueSender(string queuePath)
        {
            var sender = new AzureServiceBusQueueMessageSender(_brokeredMessageFactory, _logger, _queueManager, _retry, queuePath);
            _garbageMan.Add(sender);
            return sender;
        }

        private INimbusMessageReceiver CreateQueueReceiver(string queuePath)
        {
            var receiver = new AzureServiceBusQueueMessageReceiver(_brokeredMessageFactory,
                                                                   _queueManager,
                                                                   queuePath,
                                                                   _concurrentHandlerLimit,
                                                                   _globalHandlerThrottle,
                                                                   _logger);
            _garbageMan.Add(receiver);
            return receiver;
        }

        private INimbusMessageSender CreateTopicSender(string topicPath)
        {
            var sender = new AzureServiceBusTopicMessageSender(_brokeredMessageFactory, _logger, _queueManager, _retry, topicPath);
            _garbageMan.Add(sender);
            return sender;
        }

        private INimbusMessageReceiver CreateTopicReceiver(string topicPath, string subscriptionName, IFilterCondition filterCondition)
        {
            var receiver = new AzureServiceBusSubscriptionMessageReceiver(_queueManager,
                                                                          topicPath,
                                                                          subscriptionName,
                                                                          filterCondition,
                                                                          _concurrentHandlerLimit,
                                                                          _brokeredMessageFactory,
                                                                          _globalHandlerThrottle,
                                                                          _logger);
            _garbageMan.Add(receiver);
            return receiver;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;
            _garbageMan.Dispose();
        }
    }
}