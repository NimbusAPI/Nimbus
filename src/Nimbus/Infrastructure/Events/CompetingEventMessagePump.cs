using System;
using System.Linq;
using Microsoft.ServiceBus.Messaging;
using Nimbus.Extensions;
using Nimbus.InfrastructureContracts;

namespace Nimbus.Infrastructure.Events
{
    public class CompetingEventMessagePump : MessagePump
    {
        private readonly MessagingFactory _messagingFactory;
        private readonly ICompetingEventBroker _multicastEventBroker;
        private readonly Type _eventType;
        private readonly string _subscriptionName;
        private SubscriptionClient _client;

        public CompetingEventMessagePump(MessagingFactory messagingFactory, ICompetingEventBroker multicastEventBroker, Type eventType, string subscriptionName, ILogger logger)
            : base(logger)
        {
            _messagingFactory = messagingFactory;
            _multicastEventBroker = multicastEventBroker;
            _eventType = eventType;
            _subscriptionName = subscriptionName;
        }

        public override void Start()
        {
            var topicPath = PathFactory.TopicPathFor(_eventType);
            _client = _messagingFactory.CreateSubscriptionClient(topicPath, _subscriptionName);
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
            _multicastEventBroker.PublishCompeting((dynamic) busEvent);
        }
    }
}