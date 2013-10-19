using System;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;

namespace Nimbus
{
    internal class BusCommandSender : ICommandSender
    {
        private readonly IMessageSenderFactory _messageSenderFactory;

        public BusCommandSender(IMessageSenderFactory messageSenderFactory)
        {
            _messageSenderFactory = messageSenderFactory;
        }

        public async Task Send<TBusCommand>(TBusCommand busCommand)
        {
            var sender = _messageSenderFactory.GetMessageSender(typeof (TBusCommand));
            var message = new BrokeredMessage(busCommand);
            await sender.SendBatchAsync(new[] {message});
        }
    }

    public interface IMessageSenderFactory
    {
        MessageSender GetMessageSender(Type messageType);
    }

    internal class MessageSenderFactory : IMessageSenderFactory
    {
        private readonly MessagingFactory _messagingFactory;

        public MessageSenderFactory(MessagingFactory messagingFactory)
        {
            _messagingFactory = messagingFactory;
        }

        public MessageSender GetMessageSender(Type messageType)
        {
            return _messagingFactory.CreateMessageSender(messageType.FullName);
        }
    }
}