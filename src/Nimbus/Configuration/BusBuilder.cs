using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Nimbus.ConcurrentCollections;
using Nimbus.Configuration.Settings;
using Nimbus.Extensions;
using Nimbus.Infrastructure;
using Nimbus.Infrastructure.Commands;
using Nimbus.Infrastructure.Events;
using Nimbus.Infrastructure.RequestResponse;
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


            var namespaceManagers = Enumerable.Range(0, container.Resolve<ServerConnectionCountSetting>())
                                               .AsParallel()
                                               .Select(i =>
                                                       {
                                                           var namespaceManager = NamespaceManager.CreateFromConnectionString(container.Resolve<ConnectionStringSetting>());
                                                           namespaceManager.Settings.OperationTimeout = TimeSpan.FromSeconds(120);
                                                           return namespaceManager;
                                                       })
                                               .ToArray();
            var namespaceManagerRoundRobin = new RoundRobin<NamespaceManager>(namespaceManagers);
            container.Register<Func<NamespaceManager>>(c => namespaceManagerRoundRobin.GetNext);

            var messagingFactories = Enumerable.Range(0, container.Resolve<ServerConnectionCountSetting>())
                                               .AsParallel()
                                               .Select(i => MessagingFactory.CreateFromConnectionString(container.Resolve<ConnectionStringSetting>()))
                                               .ToArray();
            var messagingFactoryRoundRobin = new RoundRobin<MessagingFactory>(messagingFactories);
            container.Register<Func<MessagingFactory>>(c => messagingFactoryRoundRobin.GetNext);

            if (configuration.Debugging.RemoveAllExistingNamespaceElements)
            {
                RemoveAllExistingNamespaceElements(container.Resolve<Func<NamespaceManager>>(), logger);
            }

            logger.Debug("Creating message pumps and subscriptions.");
            var messagePumps = new List<IMessagePump>();
            messagePumps.Add(container.Resolve<ResponseMessagePumpFactory>().Create());
            messagePumps.AddRange(container.Resolve<CommandMessagePumpsFactory>().CreateAll());
            messagePumps.AddRange(container.Resolve<RequestMessagePumpsFactory>().CreateAll());
            messagePumps.AddRange(container.Resolve<MulticastRequestMessagePumpsFactory>().CreateAll());
            messagePumps.AddRange(container.Resolve<MulticastEventMessagePumpsFactory>().CreateAll());
            messagePumps.AddRange(container.Resolve<CompetingEventMessagePumpsFactory>().CreateAll());
            logger.Debug("Message pumps and subscriptions are all created.");

            var bus = new Bus(container.Resolve<ILogger>(),
                              container.Resolve<ICommandSender>(),
                              container.Resolve<IRequestSender>(),
                              container.Resolve<IMulticastRequestSender>(),
                              container.Resolve<IEventSender>(),
                              messagePumps,
                              container.Resolve<DeadLetterQueues>());

            bus.Starting += delegate
            {
                container.Resolve<AzureQueueManager>().WarmUp();
            };


            bus.Disposing += delegate
            {
                container.Dispose();
                messagingFactories.Do(mf => mf.Close()).Done();
            };

            logger.Debug("Bus built. Job done!");

            return bus;
        }

        /// <summary>
        ///     Danger! Danger, Will Robinson!
        /// </summary>
        private static void RemoveAllExistingNamespaceElements(Func<NamespaceManager> namespaceManager, ILogger logger)
        {
            logger.Debug("Removing all existing namespace elements. IMPORTANT: This should only be done in your regression test suites.");

            var queueDeletionTasks = namespaceManager().GetQueues()
                                                       .Select(q => q.Path)
                                                       .Select(queuePath => Task.Run(async delegate
                                                                                           {
                                                                                               logger.Debug("Deleting queue {0}", queuePath);
                                                                                               await namespaceManager().DeleteQueueAsync(queuePath);
                                                                                           }))
                                                       .ToArray();

            var topicDeletionTasks = namespaceManager().GetTopics()
                                                       .Select(t => t.Path)
                                                       .Select(topicPath => Task.Run(async delegate
                                                                                           {
                                                                                               logger.Debug("Deleting topic {0}", topicPath);
                                                                                               await namespaceManager().DeleteTopicAsync(topicPath);
                                                                                           }))
                                                       .ToArray();

            var allDeletionTasks = new Task[0]
                .Union(queueDeletionTasks)
                .Union(topicDeletionTasks)
                .ToArray();
            Task.WaitAll(allDeletionTasks);
        }
    }
}