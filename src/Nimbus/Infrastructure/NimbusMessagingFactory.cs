using System;
using System.Collections.Concurrent;
using Nimbus.Configuration;
using Nimbus.Infrastructure.MessageSendersAndReceivers;

namespace Nimbus.Infrastructure
{
    internal class NimbusMessagingFactory : INimbusMessageSenderFactory, ICreateComponents
    {
        private readonly IQueueManager _queueManager;

        private readonly ConcurrentDictionary<string, INimbusMessageSender> _messageSenders = new ConcurrentDictionary<string, INimbusMessageSender>();
        private readonly GarbageMan _garbageMan = new GarbageMan();

        public NimbusMessagingFactory(IQueueManager queueManager)
        {
            _queueManager = queueManager;
        }

        public INimbusMessageSender GetMessageSender(Type messageType)
        {
            return GetMessageSender(PathFactory.QueuePathFor(messageType));
        }

        public INimbusMessageSender GetMessageSender(string queuePath)
        {
            return _messageSenders.GetOrAdd(queuePath, CreateNimbusMessageSender);
        }

        private INimbusMessageSender CreateNimbusMessageSender(string queuePath)
        {
            var messageSender = new NimbusQueueMessageSender(_queueManager, queuePath);
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