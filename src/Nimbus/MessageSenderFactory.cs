using System;
using System.Collections.Concurrent;
using Microsoft.ServiceBus.Messaging;

namespace Nimbus
{
    internal class MessageSenderFactory : IMessageSenderFactory, IDisposable
    {
        private readonly MessagingFactory _messagingFactory;
        private readonly ConcurrentDictionary<Type, MessageSender> _messageSenders = new ConcurrentDictionary<Type, MessageSender>();

        public MessageSenderFactory(MessagingFactory messagingFactory)
        {
            _messagingFactory = messagingFactory;
        }

        public MessageSender GetMessageSender(Type messageType)
        {
            var messageSender = _messageSenders.GetOrAdd(messageType, t => _messagingFactory.CreateMessageSender(t.FullName));
            return messageSender;
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