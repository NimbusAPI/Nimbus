using System;
using System.Collections.Concurrent;
using Microsoft.ServiceBus.Messaging;

namespace Nimbus.Infrastructure
{
    [Obsolete("Consider removing in favour of just QueueManager.")]
    internal class MessageSenderFactory : IMessageSenderFactory, IDisposable
    {
        private readonly IQueueManager _queueManager;
        private readonly ConcurrentDictionary<Type, MessageSender> _messageSenders = new ConcurrentDictionary<Type, MessageSender>();

        public MessageSenderFactory(IQueueManager queueManager)
        {
            _queueManager = queueManager;
        }

        public MessageSender GetMessageSender(Type messageType)
        {
            return _messageSenders.GetOrAdd(messageType, CreateMessageSender);
        }

        private MessageSender CreateMessageSender(Type messageType)
        {
            return _queueManager.CreateMessageSender(messageType);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;

            foreach (var messageSender in _messageSenders.Values)
            {
                messageSender.Close();
            }
        }
    }
}