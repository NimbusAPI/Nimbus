using System;
using System.Collections.Concurrent;
using Nimbus.Configuration;
using Nimbus.Infrastructure.MessageSendersAndReceivers;

namespace Nimbus.Infrastructure
{
    internal class NimbusMessagingFactory : INimbusMessageSenderFactory, ICreateComponents
    {
        private readonly IQueueManager _queueManager;

        private readonly ConcurrentDictionary<string, INimbusMessageSender> _queueMessageSenders = new ConcurrentDictionary<string, INimbusMessageSender>();
        private readonly ConcurrentDictionary<string, INimbusMessageSender> _topicMessageSenders = new ConcurrentDictionary<string, INimbusMessageSender>();
        private readonly GarbageMan _garbageMan = new GarbageMan();

        public NimbusMessagingFactory(IQueueManager queueManager)
        {
            _queueManager = queueManager;
        }

        public INimbusMessageSender GetQueueSender(Type messageType)
        {
            return GetQueueSender(PathFactory.QueuePathFor(messageType));
        }

        public INimbusMessageSender GetQueueSender(string queuePath)
        {
            return _queueMessageSenders.GetOrAdd(queuePath, CreateQueueSender);
        }

        public INimbusMessageSender GetTopicSender(Type messageType)
        {
            return GetTopicSender(PathFactory.TopicPathFor(messageType));
        }

        public INimbusMessageSender GetTopicSender(string topicPath)
        {
            return _topicMessageSenders.GetOrAdd(topicPath, CreateTopicSender);
        }

        private INimbusMessageSender CreateQueueSender(string queuePath)
        {
            var messageSender = new NimbusQueueMessageSender(_queueManager, queuePath);
            _garbageMan.Add(messageSender);
            return messageSender;
        }

        private INimbusMessageSender CreateTopicSender(string topicPath)
        {
            var messageSender = new NimbusTopicMessageSender(_queueManager, topicPath);
            _garbageMan.Add(messageSender);
            return messageSender;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~NimbusMessagingFactory()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;
            _garbageMan.Dispose();
        }
    }
}