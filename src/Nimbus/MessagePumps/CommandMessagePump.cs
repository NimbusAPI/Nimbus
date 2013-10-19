using System;
using Microsoft.ServiceBus.Messaging;

namespace Nimbus.MessagePumps
{
    public class CommandMessagePump : MessagePump
    {
        private readonly MessagingFactory _messagingFactory;
        private readonly ICommandBroker _commandBroker;
        private readonly Type _messageType;
        private MessageReceiver _reciever;

        public CommandMessagePump(MessagingFactory messagingFactory, ICommandBroker commandBroker, Type messageType)
        {
            _messagingFactory = messagingFactory;
            _commandBroker = commandBroker;
            _messageType = messageType;
        }

        public override void Start()
        {
            _reciever = _messagingFactory.CreateMessageReceiver(_messageType.FullName);
            base.Start();
        }

        public override void Stop()
        {
            if (_reciever != null) _reciever.Close();
            base.Stop();
        }

        protected override void PumpMessage()
        {
            var message = _reciever.Receive(TimeSpan.FromSeconds(1));
            if (message == null) return;

            var body = message.GetBody(_messageType);
            _commandBroker.Dispatch((dynamic) body);
            message.Complete();
        }
    }
}