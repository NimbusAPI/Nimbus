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
        private string _instanceName;
        private string _connectionString;
        private ICommandBroker _commandBroker;
        private IRequestBroker _requestBroker;
        private IEventBroker _eventBroker;
        private Type[] _commandTypes;
        private Type[] _requestTypes;
        private Type[] _eventTypes;

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

        public BusBuilder()
        {
            
        }

        public Bus Build()
        {
            var customQueueName = "DefaultQueue";
            var replyQueueName = string.Format("{0}.{1}.{2}", Environment.MachineName, Process.GetCurrentProcess().ProcessName, customQueueName);

            var namespaceManager = NamespaceManager.CreateFromConnectionString(_connectionString);
            var messagingFactory = MessagingFactory.CreateFromConnectionString(_connectionString);
            var queueManager = new QueueManager(namespaceManager);
            var messagePumps = new List<IMessagePump>();
            var requestResponseCorrelator = new RequestResponseCorrelator();
            var messageSenderFactory = new MessageSenderFactory(messagingFactory);
            var topicClientFactory = new TopicClientFactory(messagingFactory);
            var commandSender = new BusCommandSender(messageSenderFactory);
            var requestSender = new BusRequestSender(messageSenderFactory, replyQueueName, requestResponseCorrelator);
            var eventSender = new BusEventSender(topicClientFactory);

            CreateRequestResponseMessagePump(messagingFactory, queueManager, replyQueueName, requestResponseCorrelator, messagePumps);
            CreateCommandMessagePumps(queueManager, messagingFactory, messagePumps);
            CreateRequestMessagePumps(queueManager, messagingFactory, messagePumps);
            CreateEventMessagePumps(queueManager, messagingFactory, messagePumps);

            var bus = new Bus(commandSender, requestSender, eventSender, messagePumps);
            return bus;
        }


        public string ConnectionString 
        {
            set { _connectionString = value; }
        }

        public string InstanceName
        {
            set { _instanceName = value; }
        }



        private void CreateRequestResponseMessagePump(MessagingFactory messagingFactory,
                                                      IQueueManager queueManager,
                                                      string replyQueueName,
                                                      RequestResponseCorrelator requestResponseCorrelator,
                                                      List<IMessagePump> messagePumps)
        {
            queueManager.EnsureQueueExists(replyQueueName);
            var requestResponseMessagePump = new ResponseMessagePump(messagingFactory, replyQueueName, requestResponseCorrelator);
            messagePumps.Add(requestResponseMessagePump);
        }

        private void CreateEventMessagePumps(IQueueManager queueManager, MessagingFactory messagingFactory, List<IMessagePump> messagePumps)
        {
            foreach (var eventType in _eventTypes)
            {
                queueManager.EnsureTopicExists(eventType);
                var subscriptionName = String.Format("{0}.{1}", Environment.MachineName, "MyApp");
                queueManager.EnsureSubscriptionExists(eventType, subscriptionName);

                var pump = new EventMessagePump(messagingFactory, _eventBroker, eventType, subscriptionName);
                messagePumps.Add(pump);
            }
        }

        private void CreateRequestMessagePumps(IQueueManager queueManager, MessagingFactory messagingFactory, List<IMessagePump> messagePumps)
        {
            foreach (var requestType in _requestTypes)
            {
                queueManager.EnsureQueueExists(requestType);

                var pump = new RequestMessagePump(messagingFactory, _requestBroker, requestType);
                messagePumps.Add(pump);
            }
        }

        private void CreateCommandMessagePumps(IQueueManager queueManager, MessagingFactory messagingFactory, List<IMessagePump> messagePumps)
        {
            foreach (var commandType in _commandTypes)
            {
                queueManager.EnsureQueueExists(commandType);

                var pump = new CommandMessagePump(messagingFactory, _commandBroker, commandType);
                messagePumps.Add(pump);
            }
        }

        public void RegisterHandlers(IRegisterHandlers registrar)
        {
            throw new NotImplementedException();
        }
    }
}