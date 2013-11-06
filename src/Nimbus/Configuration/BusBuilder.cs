using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Nimbus.Extensions;
using Nimbus.Infrastructure;
using Nimbus.InfrastructureContracts;
using Nimbus.MessagePumps;
using Nimbus.PoisonMessages;

namespace Nimbus.Configuration
{
    public class BusBuilder
    {
        private readonly BusBuilderConfiguration _configuration;

        public BusBuilder()
        {
            _configuration = new BusBuilderConfiguration();
        }

        public BusBuilderConfiguration Configure()
        {
            return _configuration;
        }

        internal static Bus Build(BusBuilderConfiguration configuration)
        {
            var replyQueueName = configuration.InstanceName;

            var namespaceManager = NamespaceManager.CreateFromConnectionString(configuration.ConnectionString);
            var messagingFactory = MessagingFactory.CreateFromConnectionString(configuration.ConnectionString);

            var queueManager = new QueueManager(namespaceManager, messagingFactory, configuration.MaxDeliveryAttempts);
            var messagePumps = new List<IMessagePump>();
            var requestResponseCorrelator = new RequestResponseCorrelator();
            var messageSenderFactory = new MessageSenderFactory(messagingFactory);
            var topicClientFactory = new TopicClientFactory(messagingFactory);
            var commandSender = new BusCommandSender(messageSenderFactory);
            var requestSender = new BusRequestSender(messageSenderFactory, replyQueueName, requestResponseCorrelator, configuration.DefaultTimeout);
            var eventSender = new BusEventSender(topicClientFactory);

            CreateMyInputQueue(queueManager, replyQueueName);
            CreateCommandQueues(configuration, queueManager);
            CreateRequestQueues(configuration, queueManager);
            CreateEventTopics(configuration, queueManager);

            CreateRequestResponseMessagePump(configuration, messagingFactory, replyQueueName, requestResponseCorrelator, messagePumps);
            CreateCommandMessagePumps(configuration, messagingFactory, messagePumps);
            CreateRequestMessagePumps(configuration, messagingFactory, messagePumps);
            CreateMulticastEventMessagePumps(configuration, queueManager, messagingFactory, messagePumps);
            CreateCompetingEventMessagePumps(configuration, queueManager, messagingFactory, messagePumps);

            var commandDeadLetterQueue = new DeadLetterQueue(queueManager);
            var requestDeadLetterQueue = new DeadLetterQueue(queueManager);
            var deadLetterQueues = new DeadLetterQueues(commandDeadLetterQueue, requestDeadLetterQueue);

            var bus = new Bus(commandSender, requestSender, eventSender, messagePumps, deadLetterQueues);
            return bus;
        }

        private static void CreateMyInputQueue(QueueManager queueManager, string replyQueueName)
        {
            queueManager.EnsureQueueExists(replyQueueName);
        }

        private static void CreateCommandQueues(BusBuilderConfiguration configuration, QueueManager queueManager)
        {
            configuration.CommandTypes
                         .AsParallel()
                         .Do(queueManager.EnsureQueueExists)
                         .Done();
        }

        private static void CreateRequestQueues(BusBuilderConfiguration configuration, QueueManager queueManager)
        {
            configuration.RequestTypes
                         .AsParallel()
                         .Do(queueManager.EnsureQueueExists)
                         .Done();
        }

        private static void CreateEventTopics(BusBuilderConfiguration configuration, QueueManager queueManager)
        {
            configuration.EventTypes
                         .AsParallel()
                         .Do(queueManager.EnsureTopicExists)
                         .Done();
        }

        private static void CreateRequestResponseMessagePump(BusBuilderConfiguration configuration,
                                                             MessagingFactory messagingFactory,
                                                             string replyQueueName,
                                                             RequestResponseCorrelator requestResponseCorrelator,
                                                             ICollection<IMessagePump> messagePumps)
        {
            var requestResponseMessagePump = new ResponseMessagePump(messagingFactory, replyQueueName, requestResponseCorrelator, configuration.Logger);
            messagePumps.Add(requestResponseMessagePump);
        }

        private static void CreateMulticastEventMessagePumps(BusBuilderConfiguration configuration,
                                                             IQueueManager queueManager,
                                                             MessagingFactory messagingFactory,
                                                             List<IMessagePump> messagePumps)
        {
            foreach (var handlerType in configuration.MulticastEventHandlerTypes)
            {
                var eventType = handlerType.GetGenericTypeParametersFor(typeof (IHandleMulticastEvent<>)).Single();

                var myInstanceSubscriptionName = String.Format("{0}.{1}", configuration.InstanceName, configuration.ApplicationName);
                queueManager.EnsureSubscriptionExists(eventType, myInstanceSubscriptionName);
                var pump = new EventMessagePump(messagingFactory, configuration.MulticastEventBroker, eventType, myInstanceSubscriptionName, configuration.Logger);
                messagePumps.Add(pump);
            }
        }

        private static void CreateCompetingEventMessagePumps(BusBuilderConfiguration configuration,
                                                             IQueueManager queueManager,
                                                             MessagingFactory messagingFactory,
                                                             List<IMessagePump> messagePumps)
        {
            foreach (var handlerType in configuration.CompetingEventHandlerTypes)
            {
                var eventType = handlerType.GetGenericTypeParametersFor(typeof(IHandleCompetingEvent<>)).Single();

                var applicationSharedSubscriptionName = String.Format("{0}", configuration.ApplicationName);
                queueManager.EnsureSubscriptionExists(eventType, applicationSharedSubscriptionName);
                var pump = new EventMessagePump(messagingFactory, configuration.CompetingEventBroker, eventType, applicationSharedSubscriptionName, configuration.Logger);
                messagePumps.Add(pump);
            }
        }

        private static void CreateRequestMessagePumps(BusBuilderConfiguration configuration, MessagingFactory messagingFactory, List<IMessagePump> messagePumps)
        {
            foreach (var requestType in configuration.RequestTypes)
            {
                var pump = new RequestMessagePump(messagingFactory, configuration.RequestBroker, requestType, configuration.Logger);
                messagePumps.Add(pump);
            }
        }

        private static void CreateCommandMessagePumps(BusBuilderConfiguration configuration, MessagingFactory messagingFactory, List<IMessagePump> messagePumps)
        {
            foreach (var commandType in configuration.CommandTypes)
            {
                var pump = new CommandMessagePump(messagingFactory, configuration.CommandBroker, commandType, configuration.Logger);
                messagePumps.Add(pump);
            }
        }
    }
}