using System;
using System.Collections.Concurrent;
using Nimbus.Configuration;
using Nimbus.Extensions;
using Nimbus.Infrastructure.MessageSendersAndReceivers;

namespace Nimbus.Infrastructure
{
    internal class NimbusMessagingFactory : INimbusMessagingFactory, ICreateComponents
    {
        private readonly IQueueManager _queueManager;

        private readonly ConcurrentDictionary<string, INimbusMessageSender> _queueMessageSenders = new ConcurrentDictionary<string, INimbusMessageSender>();
        private readonly ConcurrentDictionary<string, INimbusMessageReceiver> _queueMessageReceivers = new ConcurrentDictionary<string, INimbusMessageReceiver>();
        private readonly ConcurrentDictionary<string, INimbusMessageSender> _topicMessageSenders = new ConcurrentDictionary<string, INimbusMessageSender>();
        private readonly ConcurrentDictionary<string, INimbusMessageReceiver> _topicMessageReceivers = new ConcurrentDictionary<string, INimbusMessageReceiver>();
        private readonly GarbageMan _garbageMan = new GarbageMan();

        public NimbusMessagingFactory(IQueueManager queueManager)
        {
            _queueManager = queueManager;
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
            var sender = new NimbusQueueMessageSender(_queueManager, queuePath);
            _garbageMan.Add(sender);
            return sender;
        }

        private INimbusMessageReceiver CreateQueueReceiver(string queuePath)
        {
            var receiver = new NimbusQueueMessageReceiver(_queueManager, queuePath);
            _garbageMan.Add(receiver);
            return receiver;
        }

        private INimbusMessageSender CreateTopicSender(string topicPath)
        {
            var sender = new NimbusTopicMessageSender(_queueManager, topicPath);
            _garbageMan.Add(sender);
            return sender;
        }

        private INimbusMessageReceiver CreateTopicReceiver(string topicPath, string subscriptionName)
        {
            var receiver = new NimbusSubscriptionMessageReceiver(_queueManager, topicPath, subscriptionName);
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