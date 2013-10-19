using System;
using Microsoft.ServiceBus.Messaging;

namespace Nimbus.MessagePumps
{
    public class TopicMessagePump : IMessagePump
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

        public void Start()
        {
            _client = _messagingFactory.CreateSubscriptionClient(_eventType.FullName, _subscriptionName);
            _client.OnMessage(HandleIt);
        }

        private void HandleIt(BrokeredMessage brokeredMessage)
        {
            _eventBroker.Publish(brokeredMessage.GetBody(_eventType));
            brokeredMessage.Complete();
        }

        public void Stop()
        {
            if (_client != null) _client.Close();
        }
    }
}