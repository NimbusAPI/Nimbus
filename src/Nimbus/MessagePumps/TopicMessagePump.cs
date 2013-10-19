using System;
using Microsoft.ServiceBus.Messaging;

namespace Nimbus.MessagePumps
{
    public class TopicMessagePump : MessagePump
    {
        private readonly MessagingFactory _messagingFactory;
        private readonly IEventBroker _eventBroker;
        private readonly Type _eventType;
        private readonly string _subscriptionName;
        private SubscriptionClient _client;

        public TopicMessagePump(MessagingFactory messagingFactory, IEventBroker eventBroker, Type eventType, string subscriptionName)
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

        protected override void PumpMessage()
        {
            var messages = _client.ReceiveBatch(int.MaxValue);

            foreach (var message in messages)
            {
                _eventBroker.Publish(message.GetBody(_eventType));
                message.Complete();
            }
        }
    }
}