using System;
using System.Linq;
using Microsoft.ServiceBus.Messaging;
using Nimbus.Extensions;
using Nimbus.InfrastructureContracts;
using Nimbus.Logger;

namespace Nimbus.MessagePumps
{
    public class MulticastEventMessagePump : MessagePump
    {
        private readonly MessagingFactory _messagingFactory;
        private readonly IMulticastEventBroker _multicastEventBroker;
        private readonly Type _eventType;
        private readonly string _subscriptionName;
        private SubscriptionClient _client;

        public MulticastEventMessagePump(MessagingFactory messagingFactory, IMulticastEventBroker multicastEventBroker, Type eventType, string subscriptionName, ILogger logger)
            : base(logger)
        {
            _messagingFactory = messagingFactory;
            _multicastEventBroker = multicastEventBroker;
            _eventType = eventType;
            _subscriptionName = subscriptionName;
        }

        public override void Start()
        {
            _client = _messagingFactory.CreateSubscriptionClient(_eventType.FullName, _subscriptionName);
            base.Start();
        }

        public override void Stop()
        {
            if (_client != null) _client.Close();
            base.Stop();
        }

        protected override BrokeredMessage[] ReceiveMessages()
        {
            return _client.ReceiveBatch(int.MaxValue, TimeSpan.FromSeconds(1)).ToArray();
        }

        protected override void PumpMessage(BrokeredMessage message)
        {
            var busEvent = message.GetBody(_eventType);
            _multicastEventBroker.PublishMulticast((dynamic) busEvent);
        }
    }
}