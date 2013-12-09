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
using Nimbus.MessageContracts;
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
            var logger = configuration.Logger;

            logger.Debug("Constructing bus...");

            var replyQueueName = string.Format("InputQueue.{0}.{1}", configuration.ApplicationName, configuration.InstanceName);

            var namespaceManager = NamespaceManager.CreateFromConnectionString(configuration.ConnectionString);
            var versionInfo = namespaceManager.GetVersionInfo();

            var messagingFactory = MessagingFactory.CreateFromConnectionString(configuration.ConnectionString);

            var messagePumps = new List<IMessagePump>();

            var queueManager = new QueueManager(namespaceManager, messagingFactory, configuration.MaxDeliveryAttempts, logger);

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
                RemoveAllExistingNamespaceElements(namespaceManager, logger);
            }

            logger.Debug("Creating queues and topics");

            var queueCreationTasks =
                new[]
                {
                    Task.Run(() => CreateMyInputQueue(queueManager, replyQueueName, logger)),
                    Task.Run(() => CreateCommandQueues(configuration, queueManager, logger)),
                    Task.Run(() => CreateRequestQueues(configuration, queueManager, logger)),
                    Task.Run(() => CreateMulticastRequestTopics(configuration, queueManager, logger)),
                    Task.Run(() => CreateEventTopics(configuration, queueManager, logger))
                };
            Task.WaitAll(queueCreationTasks);
            logger.Debug("Queues and topics are all created.");

            logger.Debug("Creating message pumps and subscriptions.");
            var messagePumpCreationTasks =
                new[]
                {
                    Task.Run(() => CreateResponseMessagePump(configuration, messagingFactory, replyQueueName, requestResponseCorrelator, messagePumps, logger)),
                    Task.Run(() => CreateCommandMessagePumps(configuration, messagingFactory, messagePumps, logger)),
                    Task.Run(() => CreateRequestMessagePumps(configuration, messagingFactory, messagePumps, logger)),
                    Task.Run(() => CreateMulticastRequestMessagePumps(configuration, queueManager, messagingFactory, messagePumps, logger)),
                    Task.Run(() => CreateMulticastEventMessagePumps(configuration, queueManager, messagingFactory, messagePumps, logger)),
                    Task.Run(() => CreateCompetingEventMessagePumps(configuration, queueManager, messagingFactory, messagePumps, logger))
                };
            messagePumpCreationTasks.WaitAll();
            logger.Debug("Message pumps and subscriptions are all created.");

            var commandDeadLetterQueue = new DeadLetterQueue(queueManager);
            var requestDeadLetterQueue = new DeadLetterQueue(queueManager);
            var deadLetterQueues = new DeadLetterQueues(commandDeadLetterQueue, requestDeadLetterQueue);

            var bus = new Bus(commandSender, requestSender, multicastRequestSender, eventSender, messagePumps, deadLetterQueues);

            logger.Debug("Bus build. Job done!");

            return bus;
        }

        /// <summary>
        ///     Danger! Danger, Will Robinson!
        /// </summary>
        private static void RemoveAllExistingNamespaceElements(NamespaceManager namespaceManager, ILogger logger)
        {
            logger.Debug("Removing all existing namespace elements. IMPORTANT: This should only be done in your regression test suites.");

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

        private static void CreateMyInputQueue(QueueManager queueManager, string replyQueueName, ILogger logger)
        {
            logger.Debug("Creating our own input queue ({0})", replyQueueName);

            queueManager.EnsureQueueExists(replyQueueName);
        }

        private static void CreateCommandQueues(BusBuilderConfiguration configuration, QueueManager queueManager, ILogger logger)
        {
            logger.Debug("Creating command queues");

            configuration.CommandTypes
                         .AsParallel()
                         .Do(queueManager.EnsureQueueExists)
                         .Done();
        }

        private static void CreateRequestQueues(BusBuilderConfiguration configuration, QueueManager queueManager, ILogger logger)
        {
            logger.Debug("Creating request queues");

            configuration.RequestTypes
                         .AsParallel()
                         .Do(queueManager.EnsureQueueExists)
                         .Done();
        }

        private static void CreateMulticastRequestTopics(BusBuilderConfiguration configuration, QueueManager queueManager, ILogger logger)
        {
            logger.Debug("Creating multicast request topics");

            configuration.RequestTypes
                         .AsParallel()
                         .Do(queueManager.EnsureTopicExists)
                         .Done();
        }

        private static void CreateEventTopics(BusBuilderConfiguration configuration, QueueManager queueManager, ILogger logger)
        {
            logger.Debug("Creating event topics");

            configuration.EventTypes
                         .AsParallel()
                         .Do(queueManager.EnsureTopicExists)
                         .Done();
        }

        private static void CreateResponseMessagePump(BusBuilderConfiguration configuration,
                                                      MessagingFactory messagingFactory,
                                                      string replyQueueName,
                                                      RequestResponseCorrelator requestResponseCorrelator,
                                                      ICollection<IMessagePump> messagePumps,
                                                      ILogger logger)
        {

            var responseMessagePump = new ResponseMessagePump(messagingFactory, replyQueueName, requestResponseCorrelator, configuration.Logger);
            messagePumps.Add(responseMessagePump);
        }

        private static void CreateCommandMessagePumps(BusBuilderConfiguration configuration, MessagingFactory messagingFactory, List<IMessagePump> messagePumps, ILogger logger)
        {
            logger.Debug("Creating command message pumps");

            var commandTypes = configuration.CommandHandlerTypes.SelectMany(ht => ht.GetGenericInterfacesClosing(typeof(IHandleCommand<>)))
                                .Select(gi => gi.GetGenericArguments().First())
                                .OrderBy(t => t.FullName)
                                .Distinct()
                                .ToArray();

            foreach (var commandType in commandTypes)
            {
                logger.Debug("Registering Message Pump for Command type {0}", commandType.Name);
                var pump = new CommandMessagePump(messagingFactory, configuration.CommandBroker, commandType, configuration.Logger);
                messagePumps.Add(pump);    
            }

        }

        private static void CreateRequestMessagePumps(BusBuilderConfiguration configuration, MessagingFactory messagingFactory, List<IMessagePump> messagePumps, ILogger logger)
        {
            logger.Debug("Creating request message pumps");


            var requestTypes = configuration.RequestHandlerTypes.SelectMany(ht => ht.GetGenericInterfacesClosing(typeof(IHandleRequest<,>)))
                                            .Select(gi => gi.GetGenericArguments().First())
                                            .OrderBy(t => t.FullName)
                                            .Distinct()
                                            .ToArray();

            foreach (var requestType in requestTypes)
            {
                logger.Debug("Registering Message Pump for Request type {0}", requestType.Name);
                var pump = new RequestMessagePump(messagingFactory, configuration.RequestBroker, requestType, configuration.Logger);
                messagePumps.Add(pump);
            }

  
        }

        private static void CreateMulticastRequestMessagePumps(BusBuilderConfiguration configuration,
                                                               QueueManager queueManager,
                                                               MessagingFactory messagingFactory,
                                                               List<IMessagePump> messagePumps,
                                                               ILogger logger)
        {
            logger.Debug("Creating multicast request message pumps");

            var requestTypes = configuration.RequestHandlerTypes.SelectMany(ht => ht.GetGenericInterfacesClosing(typeof (IHandleRequest<,>)))
                                            .Select(gi => gi.GetGenericArguments().First())
                                            .OrderBy(t => t.FullName)
                                            .Distinct()
                                            .ToArray();

            foreach (var requestType in requestTypes)
            {
                logger.Debug("Registering Message Pump for Multicast Request type {0}", requestType.Name);

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
                                                             ICollection<IMessagePump> messagePumps,
                                                             ILogger logger)
        {
            logger.Debug("Creating multicast event message pumps");

            var eventTypes = configuration.MulticastEventHandlerTypes.SelectMany(ht => ht.GetGenericInterfacesClosing(typeof (IHandleMulticastEvent<>)))
                                          .Select(gi => gi.GetGenericArguments().Single())
                                          .OrderBy(t => t.FullName)
                                          .Distinct()
                                          .ToArray();

            foreach (var eventType in eventTypes)
            {
                logger.Debug("Registering Message Pump for Event type {0}", eventType.Name);

                var myInstanceSubscriptionName = String.Format("{0}.{1}", configuration.InstanceName, configuration.ApplicationName);
                queueManager.EnsureSubscriptionExists(eventType, myInstanceSubscriptionName);
                var pump = new MulticastEventMessagePump(messagingFactory, configuration.MulticastEventBroker, eventType, myInstanceSubscriptionName, configuration.Logger);
                messagePumps.Add(pump);
            }
        }

        private static void CreateCompetingEventMessagePumps(BusBuilderConfiguration configuration,
                                                             IQueueManager queueManager,
                                                             MessagingFactory messagingFactory,
                                                             ICollection<IMessagePump> messagePumps,
                                                             ILogger logger)
        {
            logger.Debug("Creating competing event message pumps");

            var eventTypes = configuration.CompetingEventHandlerTypes.SelectMany(ht => ht.GetGenericInterfacesClosing(typeof (IHandleCompetingEvent<>)))
                                          .Select(gi => gi.GetGenericArguments().Single())
                                          .OrderBy(t => t.FullName)
                                          .Distinct()
                                          .ToArray();

            foreach (var eventType in eventTypes)
            {
                logger.Debug("Registering Message Pump for Competing Event type {0}", eventType.Name);

                var applicationSharedSubscriptionName = String.Format("{0}", configuration.ApplicationName);
                queueManager.EnsureSubscriptionExists(eventType, applicationSharedSubscriptionName);
                var pump = new CompetingEventMessagePump(messagingFactory, configuration.CompetingEventBroker, eventType, applicationSharedSubscriptionName, configuration.Logger);
                messagePumps.Add(pump);
            }
        }
    }
}