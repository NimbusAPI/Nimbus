using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Nimbus.ConcurrentCollections;
using Nimbus.Configuration;
using Nimbus.Configuration.LargeMessages;
using Nimbus.Configuration.LargeMessages.Settings;
using Nimbus.Configuration.PoorMansIocContainer;
using Nimbus.Configuration.Settings;
using Nimbus.Configuration.Transport;
using Nimbus.Infrastructure;
using Nimbus.Infrastructure.LargeMessages;
using Nimbus.Infrastructure.MessageSendersAndReceivers;
using Nimbus.Transports.WindowsServiceBus.BrokeredMessages;
using Nimbus.Transports.WindowsServiceBus.DeadLetterOffice;
using Nimbus.Transports.WindowsServiceBus.DelayedDelivery;
using Nimbus.Transports.WindowsServiceBus.Extensions;
using Nimbus.Transports.WindowsServiceBus.QueueManagement;

namespace Nimbus.Transports.WindowsServiceBus
{
    public class WindowsServiceBusTransportConfiguration : TransportConfiguration
    {
        internal ConnectionStringSetting ConnectionString { get; set; }
        internal ServerConnectionCountSetting ServerConnectionCount { get; set; } = new ServerConnectionCountSetting();
        internal MaxSmallMessageSizeSetting MaxSmallMessageSize { get; set; } = new MaxSmallMessageSizeSetting();
        internal MaxLargeMessageSizeSetting MaxLargeMessageSize { get; set; } = new MaxLargeMessageSizeSetting();

        internal LargeMessageStorageConfiguration LargeMessageStorageConfiguration { get; set; } = new UnsupportedLargeMessageBodyStorageConfiguration();

        public WindowsServiceBusTransportConfiguration WithConnectionString(string connectionString)
        {
            ConnectionString = new ConnectionStringSetting {Value = connectionString};
            return this;
        }

        public WindowsServiceBusTransportConfiguration WithConnectionStringFromFile(string filename)
        {
            var connectionString = File.ReadAllText(filename).Trim();
            return WithConnectionString(connectionString);
        }

        public WindowsServiceBusTransportConfiguration WithLargeMessageStorage(LargeMessageStorageConfiguration largeMessageStorageConfiguration)
        {
            LargeMessageStorageConfiguration = largeMessageStorageConfiguration;
            return this;
        }

        public WindowsServiceBusTransportConfiguration WithServerConnectionCount(int serverConnectionCount)
        {
            ServerConnectionCount = new ServerConnectionCountSetting {Value = serverConnectionCount};
            return this;
        }

        protected override void RegisterComponents(PoorMansIoC container)
        {
            container.RegisterType<WindowsServiceBusTransport>(ComponentLifetime.SingleInstance, typeof (INimbusTransport));

            container.RegisterType<BrokeredMessageFactory>(ComponentLifetime.SingleInstance, typeof (IBrokeredMessageFactory));
            container.RegisterType<NamespaceCleanser>(ComponentLifetime.SingleInstance);
            container.RegisterType<AzureQueueManager>(ComponentLifetime.SingleInstance, typeof (IQueueManager));
            container.RegisterType<DelayedDeliveryService>(ComponentLifetime.SingleInstance, typeof (IDelayedDeliveryService));
            container.RegisterType<WindowsServiceBusDeadLetterOffice>(ComponentLifetime.SingleInstance, typeof(IDeadLetterOffice));
            container.RegisterType<NamespaceCleanser>(ComponentLifetime.SingleInstance, typeof(INamespaceCleanser));

            container.Register(c =>
                               {
                                   var namespaceManagerRoundRobin = new RoundRobin<NamespaceManager>(
                                       c.Resolve<ServerConnectionCountSetting>(),
                                       () =>
                                       {
                                           var namespaceManager = NamespaceManager.CreateFromConnectionString(c.Resolve<ConnectionStringSetting>());
                                           namespaceManager.Settings.OperationTimeout = c.Resolve<DefaultTimeoutSetting>();
                                           return namespaceManager;
                                       },
                                       nsm => false,
                                       nsm => { });

                                   return namespaceManagerRoundRobin;
                               },
                               ComponentLifetime.SingleInstance);

            container.Register<Func<NamespaceManager>>(c => c.Resolve<RoundRobin<NamespaceManager>>().GetNext, ComponentLifetime.InstancePerDependency);

            container.Register(c =>
                               {
                                   var messagingFactoryRoundRobin = new RoundRobin<MessagingFactory>(
                                       container.Resolve<ServerConnectionCountSetting>(),
                                       () =>
                                       {
                                           var messagingFactory = MessagingFactory.CreateFromConnectionString(c.Resolve<ConnectionStringSetting>());
                                           messagingFactory.PrefetchCount = c.Resolve<ConcurrentHandlerLimitSetting>();
                                           return messagingFactory;
                                       },
                                       mf => mf.IsBorked(),
                                       mf => { });

                                   return messagingFactoryRoundRobin;
                               },
                               ComponentLifetime.SingleInstance);

            container.Register<Func<MessagingFactory>>(c => c.Resolve<RoundRobin<MessagingFactory>>().GetNext, ComponentLifetime.InstancePerDependency);
        }

        public override IEnumerable<string> Validate()
        {
            yield break;
        }
    }
}