namespace Nimbus.Transports.AzureServiceBus2
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Microsoft.Azure.ServiceBus.Management;
    using Nimbus.ConcurrentCollections;
    using Nimbus.Configuration;
    using Nimbus.Configuration.LargeMessages;
    using Nimbus.Configuration.LargeMessages.Settings;
    using Nimbus.Configuration.PoorMansIocContainer;
    using Nimbus.Configuration.Settings;
    using Nimbus.Configuration.Transport;
    using Nimbus.Infrastructure;
    using Nimbus.Infrastructure.LargeMessages;
    using Nimbus.InfrastructureContracts;
    using Nimbus.Transports.AzureServiceBus2.BrokeredMessages;
    using Nimbus.Transports.AzureServiceBus2.ConnectionManagement;
    using Nimbus.Transports.AzureServiceBus2.DeadLetterOffice;
    using Nimbus.Transports.AzureServiceBus2.DelayedDelivery;
    using Nimbus.Transports.AzureServiceBus2.Filtering;
    using Nimbus.Transports.AzureServiceBus2.QueueManagement;

    public class AzureServiceBusTransportConfiguration : TransportConfiguration
    {
        internal ConnectionStringSetting ConnectionString { get; set; }
        internal ServerConnectionCountSetting ServerConnectionCount { get; set; } = new ServerConnectionCountSetting();
        internal MaxSmallMessageSizeSetting MaxSmallMessageSize { get; set; } = new MaxSmallMessageSizeSetting();
        internal MaxLargeMessageSizeSetting MaxLargeMessageSize { get; set; } = new MaxLargeMessageSizeSetting();

        internal LargeMessageStorageConfiguration LargeMessageStorageConfiguration { get; set; } = new UnsupportedLargeMessageBodyStorageConfiguration();

        public AzureServiceBusTransportConfiguration WithConnectionString(string connectionString)
        {
            this.ConnectionString = new ConnectionStringSetting {Value = connectionString};
            return this;
        }

        public AzureServiceBusTransportConfiguration WithConnectionStringFromFile(string filename)
        {
            var connectionString = File.ReadAllText(filename).Trim();
            return this.WithConnectionString(connectionString);
        }

        public AzureServiceBusTransportConfiguration WithLargeMessageStorage(LargeMessageStorageConfiguration largeMessageStorageConfiguration)
        {
            this.LargeMessageStorageConfiguration = largeMessageStorageConfiguration;
            return this;
        }

        public AzureServiceBusTransportConfiguration WithServerConnectionCount(int serverConnectionCount)
        {
            this.ServerConnectionCount = new ServerConnectionCountSetting {Value = serverConnectionCount};
            return this;
        }

        protected override void RegisterComponents(PoorMansIoC container)
        {
            container.RegisterType<AzureServiceBusTransport>(ComponentLifetime.SingleInstance, typeof (INimbusTransport));
            container.RegisterType<AzureQueueManager>(ComponentLifetime.SingleInstance, typeof (IQueueManager));
            container.RegisterType<BrokeredMessageFactory>(ComponentLifetime.SingleInstance, typeof (IBrokeredMessageFactory));
            container.RegisterType<DelayedDeliveryService>(ComponentLifetime.SingleInstance, typeof (IDelayedDeliveryService));
            container.RegisterType<AzureServiceBusDeadLetterOffice>(ComponentLifetime.SingleInstance, typeof (IDeadLetterOffice));
            container.RegisterType<SqlFilterExpressionGenerator>(ComponentLifetime.SingleInstance, typeof(ISqlFilterExpressionGenerator));
            container.RegisterType<NamespaceCleanser>(ComponentLifetime.SingleInstance, typeof (INamespaceCleanser));
            container.RegisterType<ConnectionManager>(ComponentLifetime.SingleInstance, typeof(IConnectionManager));
            container.Register(c =>
                               {
                                   var managerRoundRobin = new RoundRobin<ManagementClient>(
                                       c.Resolve<ServerConnectionCountSetting>(),
                                       () =>
                                       {
                                           var client = new ManagementClient(c.Resolve<ConnectionStringSetting>());
                                           return client;
                                           
                                       },
                                       nsm => false,
                                       nsm => { });
            
                                   return managerRoundRobin;
                               },
                               ComponentLifetime.SingleInstance);
            
            container.Register<Func<ManagementClient>>(c => c.Resolve<RoundRobin<ManagementClient>>().GetNext, ComponentLifetime.InstancePerDependency);
            
            // container.Register(c =>
            //                    {
            //                        var messagingFactoryRoundRobin = new RoundRobin<MessagingFactory>(
            //                            container.Resolve<ServerConnectionCountSetting>(),
            //                            () =>
            //                            {
            //                                var messagingFactory = MessagingFactory.CreateFromConnectionString(c.Resolve<ConnectionStringSetting>());
            //                                messagingFactory.PrefetchCount = c.Resolve<ConcurrentHandlerLimitSetting>();
            //                                return messagingFactory;
            //                            },
            //                            mf => mf.IsBorked(),
            //                            mf => mf.Dispose());
            //
            //                        return messagingFactoryRoundRobin;
            //                    },
            //                    ComponentLifetime.SingleInstance);

           // container.Register<Func<MessagingFactory>>(c => c.Resolve<RoundRobin<MessagingFactory>>().GetNext, ComponentLifetime.InstancePerDependency);
        }

        public override IEnumerable<string> Validate()
        {
            yield break;
        }
    }
}