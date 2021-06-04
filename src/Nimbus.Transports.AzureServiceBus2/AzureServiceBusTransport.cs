namespace Nimbus.Transports.AzureServiceBus2
{
    using System;
    using System.Threading.Tasks;
    using Nimbus.ConcurrentCollections;
    using Nimbus.Configuration.PoorMansIocContainer;
    using Nimbus.Configuration.Settings;
    using Nimbus.Extensions;
    using Nimbus.Infrastructure;
    using Nimbus.Infrastructure.MessageSendersAndReceivers;
    using Nimbus.Infrastructure.Retries;
    using Nimbus.InfrastructureContracts;
    using Nimbus.InfrastructureContracts.DependencyResolution;
    using Nimbus.InfrastructureContracts.Filtering.Conditions;
    using Nimbus.Transports.AzureServiceBus2.BrokeredMessages;
    using Nimbus.Transports.AzureServiceBus2.Filtering;
    using Nimbus.Transports.AzureServiceBus2.QueueManagement;
    using Nimbus.Transports.AzureServiceBus2.SendersAndRecievers;

    internal class AzureServiceBusTransport : INimbusTransport, IDisposable
    {
        private readonly IQueueManager _queueManager;
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
                                        IRetry retry,
                                        IDependencyResolver dependencyResolver,
                                        ISqlFilterExpressionGenerator sqlFilterExpressionGenerator)
        {
            this._queueManager = queueManager;
            this._retry = retry;
            this._dependencyResolver = dependencyResolver;
            this._sqlFilterExpressionGenerator = sqlFilterExpressionGenerator;
            this._brokeredMessageFactory = brokeredMessageFactory;
            this._globalHandlerThrottle = globalHandlerThrottle;
            this._concurrentHandlerLimit = concurrentHandlerLimit;
            this._logger = logger;
        }

        public async Task TestConnection()
        {
            this._logger.Debug("Azure Service Bus transport is online");
        }

        public INimbusMessageSender GetQueueSender(string queuePath)
        {
            return this._queueMessageSenders.GetOrAdd(queuePath, this.CreateQueueSender);
        }

        public INimbusMessageReceiver GetQueueReceiver(string queuePath)
        {
            return this._queueMessageReceivers.GetOrAdd(queuePath, this.CreateQueueReceiver);
        }

        public INimbusMessageSender GetTopicSender(string topicPath)
        {
            return this._topicMessageSenders.GetOrAdd(topicPath, this.CreateTopicSender);
        }

        public INimbusMessageReceiver GetTopicReceiver(string topicPath, string subscriptionName, IFilterCondition filter)
        {
            var key = "{0}/{1}".FormatWith(topicPath, subscriptionName);
            return this._topicMessageReceivers.GetOrAdd(key, k => this.CreateTopicReceiver(topicPath, subscriptionName, filter));
        }

        private INimbusMessageSender CreateQueueSender(string queuePath)
        {
            var sender = new AzureServiceBusQueueMessageSender(this._brokeredMessageFactory, this._logger, this._queueManager, this._retry, queuePath);
            this._garbageMan.Add(sender);
            return sender;
        }

        private INimbusMessageReceiver CreateQueueReceiver(string queuePath)
        {
            var receiver = new AzureServiceBusQueueMessageReceiver(this._brokeredMessageFactory,
                                                                   this._queueManager,
                                                                   queuePath,
                                                                   this._concurrentHandlerLimit,
                                                                   this._globalHandlerThrottle,
                                                                   this._logger);
            this._garbageMan.Add(receiver);
            return receiver;
        }

        private INimbusMessageSender CreateTopicSender(string topicPath)
        {
            var sender = new AzureServiceBusTopicMessageSender(this._brokeredMessageFactory, this._logger, this._queueManager, this._retry, topicPath);
            this._garbageMan.Add(sender);
            return sender;
        }

        private INimbusMessageReceiver CreateTopicReceiver(string topicPath, string subscriptionName, IFilterCondition filterCondition)
        {
            var receiver = new AzureServiceBusSubscriptionMessageReceiver(this._queueManager,
                                                                          topicPath,
                                                                          subscriptionName,
                                                                          filterCondition,
                                                                          this._concurrentHandlerLimit,
                                                                          this._brokeredMessageFactory,
                                                                          this._globalHandlerThrottle,
                                                                          this._logger);
            this._garbageMan.Add(receiver);
            return receiver;
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;
            this._garbageMan.Dispose();
        }
    }
}