using System;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;

namespace Nimbus
{
    public class MessagePump : IMessagePump
    {
        private readonly MessagingFactory _messagingFactory;
        private readonly ICommandBroker _commandBroker;
        private readonly Type _messageType;
        private MessageReceiver _reciever;

        public MessagePump(MessagingFactory messagingFactory, ICommandBroker commandBroker, Type messageType)
        {
            _messagingFactory = messagingFactory;
            _commandBroker = commandBroker;
            _messageType = messageType;
        }

        public void Start()
        {
            _reciever = _messagingFactory.CreateMessageReceiver(_messageType.FullName);

            Task.Run(() => DoWork());
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }

        private void DoWork()
        {
            while (true)
            {
                var message = _reciever.Receive();

                var body = message.GetBody(_messageType);
                _commandBroker.Dispatch((dynamic) body);
                message.Complete();
            }
        }
    }
}