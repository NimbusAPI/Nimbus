using System;
using System.Linq;
using Microsoft.ServiceBus.Messaging;
using Nimbus.Extensions;
using Nimbus.InfrastructureContracts;
using Nimbus.Logger;

namespace Nimbus.MessagePumps
{
    public class TimeoutMessagePump : MessagePump
    {
        private readonly MessagingFactory _messagingFactory;
        private readonly ITimeoutBroker _timeoutBroker;
        private readonly Type _messageType;
        private MessageReceiver _reciever;

        public TimeoutMessagePump(MessagingFactory messagingFactory, ITimeoutBroker timeoutBroker, Type messageType, ILogger logger) : base(logger)
        {
            _messagingFactory = messagingFactory;
            _timeoutBroker = timeoutBroker;
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

        protected override BrokeredMessage[] ReceiveMessages()
        {
            return _reciever.ReceiveBatch(int.MaxValue, TimeSpan.FromSeconds(1)).ToArray();
        }

        protected override void PumpMessage(BrokeredMessage message)
        {
            var busTimeout = message.GetBody(_messageType);
            _timeoutBroker.Dispatch((dynamic) busTimeout);
        }
    }
}