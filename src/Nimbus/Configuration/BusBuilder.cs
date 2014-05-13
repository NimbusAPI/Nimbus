using System;
using System.Linq;
using System.Reflection;
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

            RegisterPropertiesFromConfigurationObject(container, configuration);
            RegisterPropertiesFromConfigurationObject(container, configuration.LargeMessageStorageConfiguration);
            RegisterPropertiesFromConfigurationObject(container, configuration.Debugging);

            var namespaceManagers =
                Enumerable.Range(0, container.Resolve<ServerConnectionCountSetting>())
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
                                               .Select(i =>
                                                       {
                                                           var messagingFactory = MessagingFactory.CreateFromConnectionString(container.Resolve<ConnectionStringSetting>());
                                                           messagingFactory.PrefetchCount = 20;
                                                           return messagingFactory;
                                                       })
                                               .ToArray();
            var messagingFactoryRoundRobin = new RoundRobin<MessagingFactory>(messagingFactories);
            container.Register<Func<MessagingFactory>>(c => messagingFactoryRoundRobin.GetNext);

            if (configuration.Debugging.RemoveAllExistingNamespaceElements)
            {
                var namespaceCleanser = container.Resolve<NamespaceCleanser>();
                namespaceCleanser.RemoveAllExistingNamespaceElements().Wait();
            }

            logger.Debug("Creating message pumps and subscriptions.");

            var messagePumps = new MessagePumpsManager(
                container.Resolve<ResponseMessagePumpFactory>().Create(),
                container.Resolve<RequestMessagePumpsFactory>().CreateAll(),
                container.Resolve<CommandMessagePumpsFactory>().CreateAll(),
                container.Resolve<MulticastRequestMessagePumpsFactory>().CreateAll(),
                container.Resolve<MulticastEventMessagePumpsFactory>().CreateAll(),
                container.Resolve<CompetingEventMessagePumpsFactory>().CreateAll());

            logger.Debug("Message pumps and subscriptions are all created.");

            var bus = new Bus(container.Resolve<ILogger>(),
                              container.Resolve<ICommandSender>(),
                              container.Resolve<IRequestSender>(),
                              container.Resolve<IMulticastRequestSender>(),
                              container.Resolve<IEventSender>(),
                              messagePumps,
                              container.Resolve<DeadLetterQueues>());

            bus.Starting += delegate { container.Resolve<AzureQueueManager>().WarmUp(); };

            bus.Disposing += delegate
                             {
                                 messagingFactories
                                     .Do(mf => mf.CloseAsync()) // don't wait
                                     .Done();
                                 container.Dispose();
                             };

            logger.Info("Bus built. Job done!");

            return bus;
        }

        private static void RegisterPropertiesFromConfigurationObject(PoorMansIoC container, object configuration)
        {
            foreach (var prop in configuration.GetType().GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance))
            {
                var value = prop.GetValue(configuration);
                if (value == null) continue;

                container.Register(value);
            }
        }
    }
}