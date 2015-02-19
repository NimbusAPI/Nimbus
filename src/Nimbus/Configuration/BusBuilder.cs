using System;
using System.Reflection;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Nimbus.ConcurrentCollections;
using Nimbus.Configuration.Settings;
using Nimbus.Infrastructure;
using Nimbus.Infrastructure.Commands;
using Nimbus.Infrastructure.Events;
using Nimbus.Infrastructure.Heartbeat;
using Nimbus.Infrastructure.PropertyInjection;
using Nimbus.Infrastructure.RequestResponse;
using Nimbus.Infrastructure.TaskScheduling;
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

            var namespaceManagerRoundRobin = new RoundRobin<NamespaceManager>(
                container.Resolve<ServerConnectionCountSetting>(),
                () =>
                {
                    var namespaceManager = NamespaceManager.CreateFromConnectionString(container.Resolve<ConnectionStringSetting>());
                    namespaceManager.Settings.OperationTimeout = TimeSpan.FromSeconds(120);
                    return namespaceManager;
                },
                nsm => false,
                nsm => { });

            container.Register<Func<NamespaceManager>>(c => namespaceManagerRoundRobin.GetNext);

            var messagingFactoryRoundRobin = new RoundRobin<MessagingFactory>(
                container.Resolve<ServerConnectionCountSetting>(),
                () =>
                {
                    var messagingFactory = MessagingFactory.CreateFromConnectionString(container.Resolve<ConnectionStringSetting>());
                    messagingFactory.PrefetchCount = container.Resolve<ConcurrentHandlerLimitSetting>();
                    return messagingFactory;
                },
                mf => mf.IsBorked(),
                mf => { });

            container.Register<Func<MessagingFactory>>(c => messagingFactoryRoundRobin.GetNext);

            if (configuration.Debugging.RemoveAllExistingNamespaceElements)
            {
                var namespaceCleanser = container.Resolve<NamespaceCleanser>();
                namespaceCleanser.RemoveAllExistingNamespaceElements().Wait();
            }

            logger.Debug("Creating message pumps...");

            var messagePumps = new MessagePumpsManager(
                container.Resolve<ResponseMessagePumpFactory>().Create(),
                container.Resolve<RequestMessagePumpsFactory>().CreateAll(),
                container.Resolve<CommandMessagePumpsFactory>().CreateAll(),
                container.Resolve<MulticastRequestMessagePumpsFactory>().CreateAll(),
                container.Resolve<MulticastEventMessagePumpsFactory>().CreateAll(),
                container.Resolve<CompetingEventMessagePumpsFactory>().CreateAll(),
                container.Resolve<INimbusTaskFactory>());

            logger.Debug("Message pumps are all created.");

            var bus = new Bus(container.Resolve<ILogger>(),
                              container.Resolve<ICommandSender>(),
                              container.Resolve<IRequestSender>(),
                              container.Resolve<IMulticastRequestSender>(),
                              container.Resolve<IEventSender>(),
                              messagePumps,
                              container.Resolve<DeadLetterQueues>(),
                              container.Resolve<INimbusTaskFactory>(),
                              container.Resolve<IHeartbeat>());

            bus.Starting += delegate
                            {
                                if (configuration.WarmUpAzureQueueManagerDuringStartup)
                                {
                                    container.Resolve<AzureQueueManager>().WarmUp();
                                }
                                container.Resolve<PropertyInjector>().Bus = bus;
                            };
            bus.Disposing += delegate { container.Dispose(); };

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