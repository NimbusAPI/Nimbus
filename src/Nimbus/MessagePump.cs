using System;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;

namespace Nimbus
{
    public class MessagePump : IMessagePump
    {
        private readonly MessagingFactory _messagingFactory;
        private readonly IEventBroker _eventBroker;
        private readonly Type _messageType;
        private MessageReceiver _reciever;

        public MessagePump(MessagingFactory messagingFactory, IEventBroker eventBroker, Type messageType)
        {
            _messagingFactory = messagingFactory;
            _eventBroker = eventBroker;
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
                _eventBroker.Publish((dynamic) body);
                message.Complete();
            }
        }
    }

}