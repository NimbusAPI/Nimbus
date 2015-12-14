using System;
using System.Linq;
using System.Reflection;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Nimbus.ConcurrentCollections;
using Nimbus.Configuration.PoorMansIocContainer;
using Nimbus.Configuration.Settings;
using Nimbus.Extensions;
using Nimbus.Infrastructure;
using Nimbus.Infrastructure.Commands;
using Nimbus.Infrastructure.Dispatching;
using Nimbus.Infrastructure.Events;
using Nimbus.Infrastructure.Heartbeat;
using Nimbus.Infrastructure.MessageSendersAndReceivers;
using Nimbus.Infrastructure.NimbusMessageServices;
using Nimbus.Infrastructure.PropertyInjection;
using Nimbus.Infrastructure.RequestResponse;
using Nimbus.Interceptors.Inbound;
using Nimbus.Interceptors.Outbound;
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

            // Register settings types as singletons
            typeof (Bus).Assembly
                        .DefinedTypes
                        .Where(t => typeof (IValidatableConfigurationSetting).IsAssignableFrom(t))
                        .Where(t => t.IsInstantiable())
                        .Do(t => container.RegisterType(t, ComponentLifetime.SingleInstance, t))
                        .Done();

            container.RegisterType<RequestResponseCorrelator>(ComponentLifetime.SingleInstance);
            container.RegisterType<NamespaceCleanser>(ComponentLifetime.SingleInstance);
            container.RegisterType<CommandMessagePumpsFactory>(ComponentLifetime.SingleInstance);
            container.RegisterType<RequestMessagePumpsFactory>(ComponentLifetime.SingleInstance);
            container.RegisterType<ResponseMessagePumpFactory>(ComponentLifetime.SingleInstance);
            container.RegisterType<MulticastRequestMessagePumpsFactory>(ComponentLifetime.SingleInstance);
            container.RegisterType<MulticastEventMessagePumpsFactory>(ComponentLifetime.SingleInstance);
            container.RegisterType<CompetingEventMessagePumpsFactory>(ComponentLifetime.SingleInstance);
            container.RegisterType<BrokeredMessageFactory>(ComponentLifetime.SingleInstance, typeof (IBrokeredMessageFactory));
            container.RegisterType<SystemClock>(ComponentLifetime.SingleInstance, typeof (IClock));
            container.RegisterType<DispatchContextManager>(ComponentLifetime.SingleInstance, typeof (IDispatchContextManager));
            container.RegisterType<AzureQueueManager>(ComponentLifetime.SingleInstance, typeof (IQueueManager));
            container.RegisterType<ResponseMessageDispatcher>(ComponentLifetime.SingleInstance);
            container.RegisterType<MessagePump>(ComponentLifetime.InstancePerDependency);
            container.RegisterType<HandlerMapper>(ComponentLifetime.SingleInstance, typeof (IHandlerMapper));
            container.RegisterType<MessageDispatcherFactory>(ComponentLifetime.SingleInstance, typeof (IMessageDispatcherFactory));
            container.RegisterType<InboundInterceptorFactory>(ComponentLifetime.SingleInstance, typeof (IInboundInterceptorFactory));
            container.RegisterType<OutboundInterceptorFactory>(ComponentLifetime.SingleInstance, typeof (IOutboundInterceptorFactory));
            container.RegisterType<PropertyInjector>(ComponentLifetime.SingleInstance, typeof (IPropertyInjector));
            container.RegisterType<NimbusMessageFactory>(ComponentLifetime.SingleInstance, typeof (INimbusMessageFactory));
            container.RegisterType<BusCommandSender>(ComponentLifetime.SingleInstance, typeof (ICommandSender));
            container.RegisterType<BusRequestSender>(ComponentLifetime.SingleInstance, typeof (IRequestSender));
            container.RegisterType<BusMulticastRequestSender>(ComponentLifetime.SingleInstance, typeof (IMulticastRequestSender));
            container.RegisterType<BusEventSender>(ComponentLifetime.SingleInstance, typeof (IEventSender));
            container.RegisterType<KnownMessageTypeVerifier>(ComponentLifetime.SingleInstance, typeof (IKnownMessageTypeVerifier));
            container.RegisterType<DeadLetterQueues>(ComponentLifetime.SingleInstance, typeof (DeadLetterQueues), typeof (IDeadLetterQueues));
            container.RegisterType<DeadLetterQueue>(ComponentLifetime.SingleInstance, typeof (IDeadLetterQueue));
            container.RegisterType<Heartbeat>(ComponentLifetime.SingleInstance, typeof(IHeartbeat));
            container.RegisterType<Bus>(ComponentLifetime.SingleInstance);

            container.RegisterType<WindowsServiceBusTransport>(ComponentLifetime.SingleInstance, typeof(INimbusTransport));

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

            var messagePumpsManager = new MessagePumpsManager(
                container.Resolve<ResponseMessagePumpFactory>().Create(),
                container.Resolve<RequestMessagePumpsFactory>().CreateAll(),
                container.Resolve<CommandMessagePumpsFactory>().CreateAll(),
                container.Resolve<MulticastRequestMessagePumpsFactory>().CreateAll(),
                container.Resolve<MulticastEventMessagePumpsFactory>().CreateAll(),
                container.Resolve<CompetingEventMessagePumpsFactory>().CreateAll());

            logger.Debug("Message pumps are all created.");

            var bus = container.ResolveWithOverrides<Bus>(messagePumpsManager);

            bus.Starting += delegate
                            {
                                container.Resolve<AzureQueueManager>().WarmUp();
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