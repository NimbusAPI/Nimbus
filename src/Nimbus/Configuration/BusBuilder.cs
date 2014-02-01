using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Nimbus.Configuration.Settings;
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
            var logger = configuration.Logger;
            logger.Debug("Constructing bus...");

            var container = new PoorMansIoC();

            foreach (var prop in configuration.GetType().GetProperties(BindingFlags.NonPublic | BindingFlags.Instance))
            {
                var value = prop.GetValue(configuration);
                if (value == null) continue;
                container.Register(value);
            }

            container.Register(c => NamespaceManager.CreateFromConnectionString(c.Resolve<ConnectionStringSetting>()));
            container.Register(c => MessagingFactory.CreateFromConnectionString(c.Resolve<ConnectionStringSetting>()));

            var messagePumps = new List<IMessagePump>();

            if (configuration.Debugging.RemoveAllExistingNamespaceElements)
            {
                RemoveAllExistingNamespaceElements(container.Resolve<NamespaceManager>(), logger);
            }

            //EnsureQueuesAndTopicsExist(configuration, logger, queueManager, replyQueueName);

            logger.Debug("Creating message pumps and subscriptions.");
            messagePumps.Add(container.Resolve<ResponseMessagePumpFactory>().Create());
            messagePumps.AddRange(container.Resolve<CommandMessagePumpsFactory>().CreateAll());
            messagePumps.AddRange(container.Resolve<RequestMessagePumpsFactory>().CreateAll());
            messagePumps.AddRange(container.Resolve<MulticastRequestMessagePumpsFactory>().CreateAll());
            messagePumps.AddRange(container.Resolve<MulticastEventMessagePumpsFactory>().CreateAll());
            messagePumps.AddRange(container.Resolve<CompetingEventMessagePumpsFactory>().CreateAll());
            logger.Debug("Message pumps and subscriptions are all created.");

            var bus = new Bus(container.Resolve<ICommandSender>(),
                              container.Resolve<IRequestSender>(),
                              container.Resolve<IMulticastRequestSender>(),
                              container.Resolve<IEventSender>(),
                              messagePumps,
                              container.Resolve<DeadLetterQueues>());

            logger.Debug("Bus built. Job done!");

            return bus;
        }

        #region Queue plumbing. Refactor.

        private static void EnsureQueuesAndTopicsExist(BusBuilderConfiguration configuration, ILogger logger, AzureQueueManager queueManager, string replyQueueName)
        {
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
        }

        /// <summary>
        ///     Danger! Danger, Will Robinson!
        /// </summary>
        private static void RemoveAllExistingNamespaceElements(NamespaceManager namespaceManager, ILogger logger)
        {
            logger.Debug("Removing all existing namespace elements. IMPORTANT: This should only be done in your regression test suites.");

            var queueDeletionTasks = namespaceManager.GetQueues()
                                                     .Select(q => q.Path)
                                                     .Select(queuePath => Task.Run(delegate
                                                                                   {
                                                                                       logger.Debug("Deleting queue {0}", queuePath);
                                                                                       namespaceManager.DeleteQueue(queuePath);
                                                                                   }))
                                                     .ToArray();

            var topicDeletionTasks = namespaceManager.GetTopics()
                                                     .Select(t => t.Path)
                                                     .Select(topicPath => Task.Run(delegate
                                                                                   {
                                                                                       logger.Debug("Deleting topic {0}", topicPath);
                                                                                       namespaceManager.DeleteTopic(topicPath);
                                                                                   }))
                                                     .ToArray();

            queueDeletionTasks.WaitAll();
            topicDeletionTasks.WaitAll();
        }

        private static void CreateMyInputQueue(IQueueManager queueManager, string replyQueueName, ILogger logger)
        {
            logger.Debug("Creating our own input queue ({0})", replyQueueName);

            queueManager.EnsureQueueExists(replyQueueName);
        }

        private static void CreateCommandQueues(BusBuilderConfiguration configuration, IQueueManager queueManager, ILogger logger)
        {
            logger.Debug("Creating command queues");

            configuration.CommandTypes.Value
                         .AsParallel()
                         .Do(queueManager.EnsureQueueExists)
                         .Done();
        }

        private static void CreateRequestQueues(BusBuilderConfiguration configuration, IQueueManager queueManager, ILogger logger)
        {
            logger.Debug("Creating request queues");

            configuration.RequestTypes.Value
                         .AsParallel()
                         .Do(queueManager.EnsureQueueExists)
                         .Done();
        }

        private static void CreateMulticastRequestTopics(BusBuilderConfiguration configuration, IQueueManager queueManager, ILogger logger)
        {
            logger.Debug("Creating multicast request topics");

            configuration.RequestTypes.Value
                         .AsParallel()
                         .Do(queueManager.EnsureTopicExists)
                         .Done();
        }

        private static void CreateEventTopics(BusBuilderConfiguration configuration, IQueueManager queueManager, ILogger logger)
        {
            logger.Debug("Creating event topics");

            configuration.EventTypes.Value
                         .AsParallel()
                         .Do(queueManager.EnsureTopicExists)
                         .Done();
        }

        #endregion
    }
}