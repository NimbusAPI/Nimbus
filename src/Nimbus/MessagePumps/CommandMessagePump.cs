using System;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;

namespace Nimbus.MessagePumps
{
    public class CommandMessagePump : IMessagePump
    {
        private readonly MessagingFactory _messagingFactory;
        private readonly ICommandBroker _commandBroker;
        private readonly Type _messageType;
        private MessageReceiver _reciever;
        private bool _haveBeenToldToStop;

        public CommandMessagePump(MessagingFactory messagingFactory, ICommandBroker commandBroker, Type messageType)
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
            _haveBeenToldToStop = true;
        }

        private void DoWork()
        {
            while (! _haveBeenToldToStop)
            {
                var message = _reciever.Receive(TimeSpan.FromSeconds(1));
                if (message == null) continue;

                var body = message.GetBody(_messageType);
                _commandBroker.Dispatch((dynamic) body);
                message.Complete();
            }
        }
    }
}