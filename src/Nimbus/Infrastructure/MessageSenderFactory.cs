using System;
using System.Collections.Concurrent;
using Nimbus.Infrastructure.MessageSendersAndReceivers;

namespace Nimbus.Infrastructure
{
    [Obsolete("Consider removing in favour of just QueueManager.")]
    internal class MessageSenderFactory : IMessageSenderFactory
    {
        private readonly IQueueManager _queueManager;
        private readonly ConcurrentDictionary<Type, INimbusMessageSender> _messageSenders = new ConcurrentDictionary<Type, INimbusMessageSender>();

        public MessageSenderFactory(IQueueManager queueManager)
        {
            _queueManager = queueManager;
        }

        public INimbusMessageSender GetMessageSender(Type messageType)
        {
            return _messageSenders.GetOrAdd(messageType, CreateMessageSender);
        }

        private INimbusMessageSender CreateMessageSender(Type messageType)
        {
            return new NimbusQueueMessageSender(_queueManager, PathFactory.QueuePathFor(messageType));
        }
    }
}