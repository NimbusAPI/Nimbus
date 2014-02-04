using System;
using System.Collections.Concurrent;
using Nimbus.Infrastructure.MessageSendersAndReceivers;

namespace Nimbus.Infrastructure
{
    internal class NimbusMessagingFactory : INimbusMessageSenderFactory
    {
        private readonly IQueueManager _queueManager;

        private readonly ConcurrentDictionary<string, INimbusMessageSender> _messageSenders = new ConcurrentDictionary<string, INimbusMessageSender>();

        public NimbusMessagingFactory(IQueueManager queueManager)
        {
            _queueManager = queueManager;
        }

        //FIXME not sure that this belongs here. It doesn't actually need to know about Azure...
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
            return new NimbusQueueMessageSender(_queueManager, queuePath);
        }
    }
}