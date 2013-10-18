using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;

namespace Nimbus
{
    public interface IMessagePump
    {
        void Start();
        void Stop();
    }

    public class MessagePump : IMessagePump
    {
        private readonly MessagingFactory _messagingFactory;
        private readonly IEventBroker _eventBroker;
        private readonly Type _messageType;
        private MessageReceiver _reciever;
        private readonly MethodInfo _getBodyMethod;

        public MessagePump(MessagingFactory messagingFactory, IEventBroker eventBroker, Type messageType)
        {
            _messagingFactory = messagingFactory;
            _eventBroker = eventBroker;
            _messageType = messageType;

            var getBodyOpenGenericMethod = typeof (BrokeredMessage).GetMethod("GetBody", new Type[0]);
            _getBodyMethod = getBodyOpenGenericMethod.MakeGenericMethod(_messageType);
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

                var body = (dynamic) _getBodyMethod.Invoke(message, null);
                _eventBroker.Publish(body);
                message.Complete();
            }
        }
    }
}