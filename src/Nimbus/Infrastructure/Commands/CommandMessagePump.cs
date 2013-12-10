using System;
using System.Linq;
using Microsoft.ServiceBus.Messaging;
using Nimbus.Extensions;
using Nimbus.InfrastructureContracts;

namespace Nimbus.Infrastructure.Commands
{
    public class CommandMessagePump : MessagePump
    {
        private readonly MessagingFactory _messagingFactory;
        private readonly ICommandBroker _commandBroker;
        private readonly Type _messageType;
        private MessageReceiver _reciever;

        public CommandMessagePump(MessagingFactory messagingFactory, ICommandBroker commandBroker, Type messageType, ILogger logger) : base(logger)
        {
            _messagingFactory = messagingFactory;
            _commandBroker = commandBroker;
            _messageType = messageType;
        }

        public override void Start()
        {
            var queueName = PathFactory.QueuePathFor(_messageType);
            _reciever = _messagingFactory.CreateMessageReceiver(queueName);
            base.Start();
        }

        public override void Stop()
        {
            if (_reciever != null) _reciever.Close();
            base.Stop();
        }

        protected override BrokeredMessage[] ReceiveMessages()
        {
            return _reciever.ReceiveBatch(int.MaxValue, BatchTimeout).ToArray();
        }

        protected override void PumpMessage(BrokeredMessage message)
        {
            var busCommand = message.GetBody(_messageType);
            _commandBroker.Dispatch((dynamic) busCommand);
        }
    }
}