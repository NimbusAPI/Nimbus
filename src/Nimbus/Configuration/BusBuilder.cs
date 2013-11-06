using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Nimbus.Extensions;
using Nimbus.Infrastructure;
using Nimbus.MessagePumps;
using Nimbus.PoisonMessages;

namespace Nimbus.Configuration
{
    public class BusBuilder
    {
        private readonly BusBuilderConfiguration _configuration;

        public BusBuilder()
        {
            _configuration = new BusBuilderConfiguration(this);
        }

        public BusBuilderConfiguration Configure()
        {
            return _configuration;
        }

        internal Bus Build()
        {
            var replyQueueName = _configuration.InstanceName;

            var namespaceManager = NamespaceManager.CreateFromConnectionString(_configuration.ConnectionString);
            var messagingFactory = MessagingFactory.CreateFromConnectionString(_configuration.ConnectionString);

            var queueManager = new QueueManager(namespaceManager, messagingFactory, _configuration.MaxDeliveryAttempts);
            var messagePumps = new List<IMessagePump>();
            var requestResponseCorrelator = new RequestResponseCorrelator();
            var messageSenderFactory = new MessageSenderFactory(messagingFactory);
            var topicClientFactory = new TopicClientFactory(messagingFactory);
            var commandSender = new BusCommandSender(messageSenderFactory);
            var requestSender = new BusRequestSender(messageSenderFactory, replyQueueName, requestResponseCorrelator, _configuration.DefaultTimeout);
            var eventSender = new BusEventSender(topicClientFactory);

            CreateMyInputQueue(queueManager, replyQueueName);
            CreateCommandQueues(queueManager);
            CreateRequestQueues(queueManager);
            CreateEventTopics(queueManager);

            CreateRequestResponseMessagePump(messagingFactory, replyQueueName, requestResponseCorrelator, messagePumps);
            CreateCommandMessagePumps(messagingFactory, messagePumps);
            CreateRequestMessagePumps(messagingFactory, messagePumps);
            CreateEventMessagePumps(queueManager, messagingFactory, messagePumps);

            var commandDeadLetterQueue = new DeadLetterQueue(queueManager);
            var requestDeadLetterQueue = new DeadLetterQueue(queueManager);
            var deadLetterQueues = new DeadLetterQueues(commandDeadLetterQueue, requestDeadLetterQueue);

            var bus = new Bus(commandSender, requestSender, eventSender, messagePumps, deadLetterQueues);
            return bus;
        }

        private void CreateMyInputQueue(QueueManager queueManager, string replyQueueName)
        {
            queueManager.EnsureQueueExists(replyQueueName);
        }

        private void CreateCommandQueues(QueueManager queueManager)
        {
            _configuration.CommandTypes
                          .AsParallel()
                          .Do(queueManager.EnsureQueueExists)
                          .Done();
        }

        private void CreateRequestQueues(QueueManager queueManager)
        {
            _configuration.RequestTypes
                          .AsParallel()
                          .Do(queueManager.EnsureQueueExists)
                          .Done();
        }

        private void CreateEventTopics(QueueManager queueManager)
        {
            _configuration.EventTypes
                          .AsParallel()
                          .Do(queueManager.EnsureTopicExists)
                          .Done();
        }

        private void CreateRequestResponseMessagePump(MessagingFactory messagingFactory,
                                                      string replyQueueName,
                                                      RequestResponseCorrelator requestResponseCorrelator,
                                                      ICollection<IMessagePump> messagePumps)
        {
            var requestResponseMessagePump = new ResponseMessagePump(messagingFactory, replyQueueName, requestResponseCorrelator, _configuration.Logger);
            messagePumps.Add(requestResponseMessagePump);
        }

        private void CreateEventMessagePumps(IQueueManager queueManager, MessagingFactory messagingFactory, List<IMessagePump> messagePumps)
        {
            foreach (var eventType in _configuration.EventTypes)
            {
                var subscriptionName = String.Format("{0}.{1}", Environment.MachineName, "MyApp");
                queueManager.EnsureSubscriptionExists(eventType, subscriptionName);

                var pump = new EventMessagePump(messagingFactory, _configuration.EventBroker, eventType, subscriptionName, _configuration.Logger);
                messagePumps.Add(pump);
            }
        }

        private void CreateRequestMessagePumps(MessagingFactory messagingFactory, List<IMessagePump> messagePumps)
        {
            foreach (var requestType in _configuration.RequestTypes)
            {
                var pump = new RequestMessagePump(messagingFactory, _configuration.RequestBroker, requestType, _configuration.Logger);
                messagePumps.Add(pump);
            }
        }

        private void CreateCommandMessagePumps(MessagingFactory messagingFactory, List<IMessagePump> messagePumps)
        {
            foreach (var commandType in _configuration.CommandTypes)
            {
                var pump = new CommandMessagePump(messagingFactory, _configuration.CommandBroker, commandType, _configuration.Logger);
                messagePumps.Add(pump);
            }
        }
    }
}