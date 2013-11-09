using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Nimbus.Extensions;
using Nimbus.Infrastructure;
using Nimbus.Infrastructure.Commands;
using Nimbus.Infrastructure.Events;
using Nimbus.Infrastructure.RequestResponse;
using Nimbus.InfrastructureContracts;
using Nimbus.PoisonMessages;

namespace Nimbus.Configuration
{
    public class BusBuilder
    {
        public BusBuilderConfiguration Configure()
        {
            return new BusBuilderConfiguration();
        }

        internal static Bus Build(BusBuilderConfiguration configuration)
        {
            var replyQueueName = string.Format("InputQueue.{0}.{1}", configuration.ApplicationName, configuration.InstanceName);

            var namespaceManager = NamespaceManager.CreateFromConnectionString(configuration.ConnectionString);
            var messagingFactory = MessagingFactory.CreateFromConnectionString(configuration.ConnectionString);

            var messagePumps = new List<IMessagePump>();

            var queueManager = new QueueManager(namespaceManager, messagingFactory, configuration.MaxDeliveryAttempts);

            var clock = new SystemClock();
            var requestResponseCorrelator = new RequestResponseCorrelator(clock);

            var messageSenderFactory = new MessageSenderFactory(messagingFactory);
            var topicClientFactory = new TopicClientFactory(messagingFactory);
            var commandSender = new BusCommandSender(messageSenderFactory);
            var requestSender = new BusRequestSender(messageSenderFactory, replyQueueName, requestResponseCorrelator, clock, configuration.DefaultTimeout);
            var multicastRequestSender = new BusMulticastRequestSender(topicClientFactory, replyQueueName, requestResponseCorrelator, clock);
            var eventSender = new BusEventSender(topicClientFactory);

            if (configuration.Debugging.RemoveAllExistingNamespaceElements)
            {
                RemoveAllExistingNamespaceElements(namespaceManager);
            }

            var queueCreationTasks = new[]
            {
                Task.Run(() => CreateMyInputQueue(queueManager, replyQueueName)),
                Task.Run(() => CreateCommandQueues(configuration, queueManager)),
                Task.Run(() => CreateRequestQueues(configuration, queueManager)),
                Task.Run(() => CreateMulticastRequestTopics(configuration, queueManager)),
                Task.Run(() => CreateEventTopics(configuration, queueManager)),
            };
            Task.WaitAll(queueCreationTasks);

            //FIXME do these in parallel
            CreateResponseMessagePump(configuration, messagingFactory, replyQueueName, requestResponseCorrelator, messagePumps);
            CreateCommandMessagePumps(configuration, messagingFactory, messagePumps);
            CreateRequestMessagePumps(configuration, messagingFactory, messagePumps);
            CreateMulticastRequestMessagePumps(configuration, queueManager, messagingFactory, messagePumps);
            CreateMulticastEventMessagePumps(configuration, queueManager, messagingFactory, messagePumps);
            CreateCompetingEventMessagePumps(configuration, queueManager, messagingFactory, messagePumps);

            var commandDeadLetterQueue = new DeadLetterQueue(queueManager);
            var requestDeadLetterQueue = new DeadLetterQueue(queueManager);
            var deadLetterQueues = new DeadLetterQueues(commandDeadLetterQueue, requestDeadLetterQueue);

            var bus = new Bus(commandSender, requestSender, multicastRequestSender, eventSender, messagePumps, deadLetterQueues);
            return bus;
        }

        /// <summary>
        ///     Danger! Danger, Will Robinson!
        /// </summary>
        private static void RemoveAllExistingNamespaceElements(NamespaceManager namespaceManager)
        {
            var tasks = new List<Task>();

            var queuePaths = namespaceManager.GetQueues().Select(q => q.Path).ToArray();
            queuePaths
                .Do(queuePath => tasks.Add(Task.Run(() => namespaceManager.DeleteQueue(queuePath))))
                .Done();

            var topicPaths = namespaceManager.GetTopics().Select(t => t.Path).ToArray();
            topicPaths
                .Do(topicPath => tasks.Add(Task.Run(() => namespaceManager.DeleteTopic(topicPath))))
                .Done();

            tasks.WaitAll();
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

        private static void CreateMulticastRequestTopics(BusBuilderConfiguration configuration, QueueManager queueManager)
        {
            configuration.RequestTypes
                         .AsParallel()
                         .Do(queueManager.EnsureTopicExists)
                         .Done();
        }

        private static void CreateEventTopics(BusBuilderConfiguration configuration, QueueManager queueManager)
        {
            configuration.EventTypes
                         .AsParallel()
                         .Do(queueManager.EnsureTopicExists)
                         .Done();
        }

        private static void CreateResponseMessagePump(BusBuilderConfiguration configuration,
                                                      MessagingFactory messagingFactory,
                                                      string replyQueueName,
                                                      RequestResponseCorrelator requestResponseCorrelator,
                                                      ICollection<IMessagePump> messagePumps)
        {
            var responseMessagePump = new ResponseMessagePump(messagingFactory, replyQueueName, requestResponseCorrelator, configuration.Logger);
            messagePumps.Add(responseMessagePump);
        }

        private static void CreateCommandMessagePumps(BusBuilderConfiguration configuration, MessagingFactory messagingFactory, List<IMessagePump> messagePumps)
        {
            foreach (var commandType in configuration.CommandTypes)
            {
                var pump = new CommandMessagePump(messagingFactory, configuration.CommandBroker, commandType, configuration.Logger);
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

        private static void CreateMulticastRequestMessagePumps(BusBuilderConfiguration configuration,
                                                               QueueManager queueManager,
                                                               MessagingFactory messagingFactory,
                                                               List<IMessagePump> messagePumps)
        {
            var requestTypes = configuration.RequestHandlerTypes.SelectMany(ht => ht.GetGenericInterfacesClosing(typeof (IHandleRequest<,>)))
                                            .Select(gi => gi.GetGenericArguments().First())
                                            .OrderBy(t => t.FullName)
                                            .Distinct()
                                            .ToArray();

            foreach (var requestType in requestTypes)
            {
                var applicationSharedSubscriptionName = String.Format("{0}", configuration.ApplicationName);
                queueManager.EnsureSubscriptionExists(requestType, applicationSharedSubscriptionName);

                var pump = new MulticastRequestMessagePump(messagingFactory,
                                                           configuration.MulticastRequestBroker,
                                                           requestType,
                                                           applicationSharedSubscriptionName,
                                                           configuration.Logger);
                messagePumps.Add(pump);
            }
        }

        private static void CreateMulticastEventMessagePumps(BusBuilderConfiguration configuration,
                                                             IQueueManager queueManager,
                                                             MessagingFactory messagingFactory,
                                                             ICollection<IMessagePump> messagePumps)
        {
            var eventTypes = configuration.MulticastEventHandlerTypes.SelectMany(ht => ht.GetGenericInterfacesClosing(typeof (IHandleMulticastEvent<>)))
                                          .Select(gi => gi.GetGenericArguments().Single())
                                          .OrderBy(t => t.FullName)
                                          .Distinct()
                                          .ToArray();

            foreach (var eventType in eventTypes)
            {
                var myInstanceSubscriptionName = String.Format("{0}.{1}", configuration.InstanceName, configuration.ApplicationName);
                queueManager.EnsureSubscriptionExists(eventType, myInstanceSubscriptionName);
                var pump = new MulticastEventMessagePump(messagingFactory, configuration.MulticastEventBroker, eventType, myInstanceSubscriptionName, configuration.Logger);
                messagePumps.Add(pump);
            }
        }

        private static void CreateCompetingEventMessagePumps(BusBuilderConfiguration configuration,
                                                             IQueueManager queueManager,
                                                             MessagingFactory messagingFactory,
                                                             ICollection<IMessagePump> messagePumps)
        {
            var eventTypes = configuration.CompetingEventHandlerTypes.SelectMany(ht => ht.GetGenericInterfacesClosing(typeof (IHandleCompetingEvent<>)))
                                          .Select(gi => gi.GetGenericArguments().Single())
                                          .OrderBy(t => t.FullName)
                                          .Distinct()
                                          .ToArray();

            foreach (var eventType in eventTypes)
            {
                var applicationSharedSubscriptionName = String.Format("{0}", configuration.ApplicationName);
                queueManager.EnsureSubscriptionExists(eventType, applicationSharedSubscriptionName);
                var pump = new CompetingEventMessagePump(messagingFactory, configuration.CompetingEventBroker, eventType, applicationSharedSubscriptionName, configuration.Logger);
                messagePumps.Add(pump);
            }
        }
    }
}