using System;
using System.Linq;
using Microsoft.ServiceBus.Messaging;
using Nimbus.Extensions;
using Nimbus.InfrastructureContracts;
using Nimbus.Logger;

namespace Nimbus.MessagePumps
{
    public class EventMessagePump : MessagePump
    {
        private readonly MessagingFactory _messagingFactory;
        private readonly IEventBroker _eventBroker;
        private readonly Type _eventType;
        private readonly string _subscriptionName;
        private SubscriptionClient _client;

        public EventMessagePump(MessagingFactory messagingFactory, IEventBroker eventBroker, Type eventType, string subscriptionName, ILogger logger) : base(logger)
        {
            _messagingFactory = messagingFactory;
            _eventBroker = eventBroker;
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
            _eventBroker.Publish((dynamic) busEvent);
        }
    }
}