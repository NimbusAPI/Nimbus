using System;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Nimbus.ConcurrentCollections;
using Nimbus.Configuration;
using Nimbus.Configuration.Debug.Settings;
using Nimbus.Configuration.PoorMansIocContainer;
using Nimbus.Configuration.Settings;
using Nimbus.Configuration.Transport;
using Nimbus.Infrastructure;
using Nimbus.Infrastructure.MessageSendersAndReceivers;

namespace Nimbus.Transports.WindowsServiceBus
{
    public class WindowsServiceBusTransportConfiguration : TransportConfiguration
    {
        protected override void RegisterComponents(PoorMansIoC container)
        {
            container.RegisterType<BrokeredMessageFactory>(ComponentLifetime.SingleInstance, typeof (IBrokeredMessageFactory));
            container.RegisterType<WindowsServiceBusTransport>(ComponentLifetime.SingleInstance, typeof (INimbusTransport));
            container.RegisterType<NamespaceCleanser>(ComponentLifetime.SingleInstance);
            container.RegisterType<AzureQueueManager>(ComponentLifetime.SingleInstance, typeof(IQueueManager));

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

            if (container.Resolve<RemoveAllExistingNamespaceElementsSetting>())
            {
                var namespaceCleanser = container.Resolve<NamespaceCleanser>();
                namespaceCleanser.RemoveAllExistingNamespaceElements().Wait();
            }
        }
    }
}