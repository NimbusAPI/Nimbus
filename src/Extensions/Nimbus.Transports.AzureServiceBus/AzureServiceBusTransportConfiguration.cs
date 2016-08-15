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
using Nimbus.Transports.AzureServiceBus.BrokeredMessages;
using Nimbus.Transports.AzureServiceBus.Configuration;
using Nimbus.Transports.AzureServiceBus.DeadLetterOffice;
using Nimbus.Transports.AzureServiceBus.DelayedDelivery;
using Nimbus.Transports.AzureServiceBus.Extensions;
using Nimbus.Transports.AzureServiceBus.Filtering;
using Nimbus.Transports.AzureServiceBus.QueueManagement;

namespace Nimbus.Transports.AzureServiceBus
{
    public class AzureServiceBusTransportConfiguration : TransportConfiguration
    {
        internal ConnectionStringSetting ConnectionString { get; set; }
        internal ServerConnectionCountSetting ServerConnectionCount { get; set; } = new ServerConnectionCountSetting();
        internal MaxSmallMessageSizeSetting MaxSmallMessageSize { get; set; } = new MaxSmallMessageSizeSetting();
        internal MaxLargeMessageSizeSetting MaxLargeMessageSize { get; set; } = new MaxLargeMessageSizeSetting();
        internal RequireRetriesToBeHandledBy RequireRetriesToBeHandledBy { get; set; } = new RequireRetriesToBeHandledBy();

        internal LargeMessageStorageConfiguration LargeMessageStorageConfiguration { get; set; } = new UnsupportedLargeMessageBodyStorageConfiguration();

        public AzureServiceBusTransportConfiguration WithConnectionString(string connectionString)
        {
            ConnectionString = new ConnectionStringSetting {Value = connectionString};
            return this;
        }

        public AzureServiceBusTransportConfiguration WithConnectionStringFromFile(string filename)
        {
            var connectionString = File.ReadAllText(filename).Trim();
            return WithConnectionString(connectionString);
        }

        public AzureServiceBusTransportConfiguration WithLargeMessageStorage(LargeMessageStorageConfiguration largeMessageStorageConfiguration)
        {
            LargeMessageStorageConfiguration = largeMessageStorageConfiguration;
            return this;
        }

        public AzureServiceBusTransportConfiguration WithServerConnectionCount(int serverConnectionCount)
        {
            ServerConnectionCount = new ServerConnectionCountSetting {Value = serverConnectionCount};
            return this;
        }

        public AzureServiceBusTransportConfiguration WithRetriesHandledBy(RetriesHandledBy retriesHandledBy)
        {
            RequireRetriesToBeHandledBy = new RequireRetriesToBeHandledBy {Value = retriesHandledBy};
            return this;
        }

        protected override void RegisterComponents(PoorMansIoC container)
        {
            container.RegisterType<AzureServiceBusTransport>(ComponentLifetime.SingleInstance, typeof(INimbusTransport));

            container.RegisterType<BrokeredMessageFactory>(ComponentLifetime.SingleInstance, typeof(IBrokeredMessageFactory));
            container.RegisterType<NamespaceCleanser>(ComponentLifetime.SingleInstance);
            container.RegisterType<AzureQueueManager>(ComponentLifetime.SingleInstance, typeof(IQueueManager));
            container.RegisterType<DelayedDeliveryService>(ComponentLifetime.SingleInstance, typeof(IDelayedDeliveryService));
            container.RegisterType<AzureServiceBusDeadLetterOffice>(ComponentLifetime.SingleInstance, typeof(IDeadLetterOffice));
            container.RegisterType<NamespaceCleanser>(ComponentLifetime.SingleInstance, typeof(INamespaceCleanser));
            container.RegisterType<SqlFilterExpressionGenerator>(ComponentLifetime.SingleInstance, typeof(ISqlFilterExpressionGenerator));

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
                                       mf => mf.Dispose());

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