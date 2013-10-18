using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Nimbus.MessagePumps;

namespace Nimbus
{
    public class Bus : IBus
    {
        private readonly NamespaceManager _namespaceManager;
        private readonly MessagingFactory _messagingFactory;
        private readonly ICommandBroker _commandBroker;
        private readonly IRequestBroker _requestBroker;
        private readonly IEventBroker _eventBroker;
        private readonly Type[] _commandTypes;
        private readonly Type[] _requestTypes;
        private readonly Type[] _eventTypes;
        private readonly IList<IMessagePump> _messagePumps = new List<IMessagePump>();
        private IRequestResponseCorrelator _correlator;
        private string _replyQueueName;

        public Bus(NamespaceManager namespaceManager,
                   MessagingFactory messagingFactory,
                   ICommandBroker commandBroker,
                   IRequestBroker requestBroker,
                   IEventBroker eventBroker,
                   Type[] commandTypes,
                   Type[] requestTypes,
                   Type[] eventTypes)
        {
            _namespaceManager = namespaceManager;
            _messagingFactory = messagingFactory;
            _commandBroker = commandBroker;
            _requestBroker = requestBroker;
            _eventBroker = eventBroker;
            _commandTypes = commandTypes;
            _requestTypes = requestTypes;
            _eventTypes = eventTypes;
        }

        public async Task Send<TBusCommand>(TBusCommand busCommand)
        {
            var sender = _messagingFactory.CreateMessageSender(typeof (TBusCommand).FullName);
            var message = new BrokeredMessage(busCommand);
            await sender.SendAsync(message);
        }

        public async Task<TResponse> Request<TRequest, TResponse>(BusRequest<TRequest, TResponse> busRequest)
        {
            var response = await _correlator.MakeCorrelatedRequest(busRequest);
            return response;
        }

        public async Task Publish<TBusEvent>(TBusEvent busEvent)
        {
            var client = _messagingFactory.CreateTopicClient(typeof (TBusEvent).FullName);
            var brokeredMessage = new BrokeredMessage(busEvent);
            await client.SendAsync(brokeredMessage);
        }

        public void Start()
        {
            var customQueueName = "DefaultQueue";
            _replyQueueName = string.Format("{0}.{1}.{2}", Environment.MachineName, Process.GetCurrentProcess().ProcessName, customQueueName);
            _correlator = new RequestResponseCorrelator(_messagingFactory, _replyQueueName);

            EnsureQueueExists(_replyQueueName);

            foreach (var commandType in _commandTypes)
            {
                EnsureQueueExists(commandType);

                var pump = new CommandMessagePump(_messagingFactory, _commandBroker, commandType);
                _messagePumps.Add(pump);
                pump.Start();
            }

            foreach (var requestType in _requestTypes)
            {
                EnsureQueueExists(requestType);

                var pump = new RequestMessagePump(_messagingFactory, _requestBroker, requestType);
                _messagePumps.Add(pump);
                pump.Start();
            }

            _correlator.Start();

            foreach (var eventType in _eventTypes)
            {
                EnsureTopicExists(eventType);
                var subscriptionName = String.Format("{0}.{1}", Environment.MachineName, "MyApp");
                EnsureSubscriptionExists(eventType, subscriptionName);

                var pump = new TopicMessagePump(_messagingFactory, _eventBroker, eventType, subscriptionName);
                _messagePumps.Add(pump);
                pump.Start();
            }
        }

        private void EnsureSubscriptionExists(Type eventType, string subscriptionName)
        {
            if (! _namespaceManager.SubscriptionExists(eventType.FullName, subscriptionName))
            {
                _namespaceManager.CreateSubscription(eventType.FullName, subscriptionName);
            }
        }

        private void EnsureTopicExists(Type eventType)
        {
            var topicName = eventType.FullName;


            if (!_namespaceManager.TopicExists(topicName))
            {
                _namespaceManager.CreateTopic(topicName);
            }
        }

        private void EnsureQueueExists(Type commandType)
        {
            EnsureQueueExists(commandType.FullName);
        }

        private void EnsureQueueExists(string queueName)
        {
            if (!_namespaceManager.QueueExists(queueName))
            {
                _namespaceManager.CreateQueue(queueName);
            }
        }

        public void Stop()
        {
            _correlator.Stop();

            foreach (var messagePump in _messagePumps)
            {
                messagePump.Stop();
            }
        }
    }
}