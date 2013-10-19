using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Nimbus.MessagePumps;

namespace Nimbus
{
    public class BusBuilder
    {
        private readonly string _connectionString;
        private readonly ICommandBroker _commandBroker;
        private readonly IRequestBroker _requestBroker;
        private readonly IEventBroker _eventBroker;
        private readonly Type[] _commandTypes;
        private readonly Type[] _requestTypes;
        private readonly Type[] _eventTypes;

        public BusBuilder(string connectionString,
                          ICommandBroker commandBroker,
                          IRequestBroker requestBroker,
                          IEventBroker eventBroker,
                          Type[] commandTypes,
                          Type[] requestTypes,
                          Type[] eventTypes)
        {
            _connectionString = connectionString;
            _commandBroker = commandBroker;
            _requestBroker = requestBroker;
            _eventBroker = eventBroker;
            _commandTypes = commandTypes;
            _requestTypes = requestTypes;
            _eventTypes = eventTypes;
        }

        public Bus Build()
        {
            var customQueueName = "DefaultQueue";
            var replyQueueName = string.Format("{0}.{1}.{2}", Environment.MachineName, Process.GetCurrentProcess().ProcessName, customQueueName);

            var namespaceManager = NamespaceManager.CreateFromConnectionString(_connectionString);
            var messagingFactory = MessagingFactory.CreateFromConnectionString(_connectionString);

            var messagePumps = new List<IMessagePump>();

            var requestResponseCorrelator = new RequestResponseCorrelator();

            var commandSender = new BusCommandSender(messagingFactory);
            var requestSender = new BusRequestSender(messagingFactory, replyQueueName, requestResponseCorrelator);
            var eventSender = new BusEventSender(messagingFactory);

            CreateRequestResponseMessagePump(messagingFactory, namespaceManager, replyQueueName, requestResponseCorrelator, messagePumps);
            CreateCommandMessagePumps(namespaceManager, messagingFactory, messagePumps);
            CreateRequestMessagePumps(namespaceManager, messagingFactory, messagePumps);
            CreateEventMessagePumps(namespaceManager, messagingFactory, messagePumps);

            var bus = new Bus(commandSender, requestSender, eventSender, messagePumps);
            return bus;
        }

        private void CreateRequestResponseMessagePump(MessagingFactory messagingFactory,
                                                      NamespaceManager namespaceManager,
                                                      string replyQueueName,
                                                      RequestResponseCorrelator requestResponseCorrelator,
                                                      List<IMessagePump> messagePumps)
        {
            EnsureQueueExists(replyQueueName, namespaceManager);
            var requestResponseMessagePump = new ResponseMessagePump(messagingFactory, replyQueueName, requestResponseCorrelator);
            messagePumps.Add(requestResponseMessagePump);
        }

        private void CreateEventMessagePumps(NamespaceManager namespaceManager, MessagingFactory messagingFactory, List<IMessagePump> messagePumps)
        {
            foreach (var eventType in _eventTypes)
            {
                EnsureTopicExists(eventType, namespaceManager);
                var subscriptionName = String.Format("{0}.{1}", Environment.MachineName, "MyApp");
                EnsureSubscriptionExists(eventType, subscriptionName, namespaceManager);

                var pump = new TopicMessagePump(messagingFactory, _eventBroker, eventType, subscriptionName);
                messagePumps.Add(pump);
            }
        }

        private void CreateRequestMessagePumps(NamespaceManager namespaceManager, MessagingFactory messagingFactory, List<IMessagePump> messagePumps)
        {
            foreach (var requestType in _requestTypes)
            {
                EnsureQueueExists(requestType, namespaceManager);

                var pump = new RequestMessagePump(messagingFactory, _requestBroker, requestType);
                messagePumps.Add(pump);
            }
        }

        private void CreateCommandMessagePumps(NamespaceManager namespaceManager, MessagingFactory messagingFactory, List<IMessagePump> messagePumps)
        {
            foreach (var commandType in _commandTypes)
            {
                EnsureQueueExists(commandType, namespaceManager);

                var pump = new CommandMessagePump(messagingFactory, _commandBroker, commandType);
                messagePumps.Add(pump);
            }
        }

        private void EnsureSubscriptionExists(Type eventType, string subscriptionName, NamespaceManager namespaceManager)
        {
            if (!namespaceManager.SubscriptionExists(eventType.FullName, subscriptionName))
            {
                namespaceManager.CreateSubscription(eventType.FullName, subscriptionName);
            }
        }

        private void EnsureTopicExists(Type eventType, NamespaceManager namespaceManager)
        {
            var topicName = eventType.FullName;

            if (!namespaceManager.TopicExists(topicName))
            {
                namespaceManager.CreateTopic(topicName);
            }
        }

        private void EnsureQueueExists(Type commandType, NamespaceManager namespaceManager)
        {
            EnsureQueueExists(commandType.FullName, namespaceManager);
        }

        private void EnsureQueueExists(string queueName, NamespaceManager namespaceManager)
        {
            if (!namespaceManager.QueueExists(queueName))
            {
                namespaceManager.CreateQueue(queueName);
            }
        }
    }
}