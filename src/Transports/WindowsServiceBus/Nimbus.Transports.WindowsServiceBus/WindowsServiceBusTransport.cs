using System;
using Nimbus.ConcurrentCollections;
using Nimbus.Configuration.PoorMansIocContainer;
using Nimbus.Configuration.Settings;
using Nimbus.Extensions;
using Nimbus.Infrastructure;
using Nimbus.Infrastructure.MessageSendersAndReceivers;
using Nimbus.Transports.WindowsServiceBus.BrokeredMessages;
using Nimbus.Transports.WindowsServiceBus.SendersAndRecievers;

namespace Nimbus.Transports.WindowsServiceBus
{
    internal class WindowsServiceBusTransport : INimbusTransport, IDisposable
    {
        private readonly IQueueManager _queueManager;

        private readonly ThreadSafeDictionary<string, INimbusMessageSender> _queueMessageSenders = new ThreadSafeDictionary<string, INimbusMessageSender>();
        private readonly ThreadSafeDictionary<string, INimbusMessageReceiver> _queueMessageReceivers = new ThreadSafeDictionary<string, INimbusMessageReceiver>();
        private readonly ThreadSafeDictionary<string, INimbusMessageSender> _topicMessageSenders = new ThreadSafeDictionary<string, INimbusMessageSender>();
        private readonly ThreadSafeDictionary<string, INimbusMessageReceiver> _topicMessageReceivers = new ThreadSafeDictionary<string, INimbusMessageReceiver>();
        private readonly GarbageMan _garbageMan = new GarbageMan();
        private readonly ConcurrentHandlerLimitSetting _concurrentHandlerLimit;
        private readonly IBrokeredMessageFactory _brokeredMessageFactory;
        private readonly ILogger _logger;

        public WindowsServiceBusTransport(ConcurrentHandlerLimitSetting concurrentHandlerLimit,
                                          IBrokeredMessageFactory brokeredMessageFactory,
                                          ILogger logger,
                                          IQueueManager queueManager)
        {
            _queueManager = queueManager;
            _brokeredMessageFactory = brokeredMessageFactory;
            _concurrentHandlerLimit = concurrentHandlerLimit;
            _logger = logger;
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

        public INimbusMessageReceiver GetTopicReceiver(string topicPath, string subscriptionName)
        {
            var key = "{0}/{1}".FormatWith(topicPath, subscriptionName);
            return _topicMessageReceivers.GetOrAdd(key, k => CreateTopicReceiver(topicPath, subscriptionName));
        }

        private INimbusMessageSender CreateQueueSender(string queuePath)
        {
            var sender = new WindowsServiceBusQueueMessageSender(_brokeredMessageFactory, _queueManager, queuePath, _logger);
            _garbageMan.Add(sender);
            return sender;
        }

        private INimbusMessageReceiver CreateQueueReceiver(string queuePath)
        {
            var receiver = new WindowsServiceBusQueueMessageReceiver(_brokeredMessageFactory, _queueManager, queuePath, _concurrentHandlerLimit, _logger);
            _garbageMan.Add(receiver);
            return receiver;
        }

        private INimbusMessageSender CreateTopicSender(string topicPath)
        {
            var sender = new WindowsServiceBusTopicMessageSender(_brokeredMessageFactory, _queueManager, topicPath, _logger);
            _garbageMan.Add(sender);
            return sender;
        }

        private INimbusMessageReceiver CreateTopicReceiver(string topicPath, string subscriptionName)
        {
            var receiver = new WindowsServiceBusSubscriptionMessageReceiver(_queueManager, topicPath, subscriptionName, _concurrentHandlerLimit, _brokeredMessageFactory, _logger);
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