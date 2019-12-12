using System.Collections.Generic;
using Nimbus.Configuration;
using Nimbus.Configuration.PoorMansIocContainer;
using Nimbus.Configuration.Transport;
using Nimbus.Infrastructure;
using Nimbus.Infrastructure.LargeMessages;
using Nimbus.InfrastructureContracts;
using Nimbus.Transports.InProcess.DeadLetterOffice;
using Nimbus.Transports.InProcess.DelayedDelivery;
using Nimbus.Transports.InProcess.MessageSendersAndReceivers;
using Nimbus.Transports.InProcess.QueueManagement;

namespace Nimbus.Transports.InProcess
{
    public class InProcessTransportConfiguration : TransportConfiguration
    {
        protected override void RegisterComponents(PoorMansIoC container)
        {
            container.RegisterType<InProcessMessageStore>(ComponentLifetime.SingleInstance);

            container.RegisterType<InProcessQueueSender>(ComponentLifetime.InstancePerDependency);
            container.RegisterType<InProcessTopicSender>(ComponentLifetime.InstancePerDependency);
            container.RegisterType<InProcessQueueReceiver>(ComponentLifetime.InstancePerDependency);
            container.RegisterType<InProcessSubscriptionReceiver>(ComponentLifetime.InstancePerDependency);
            container.RegisterType<InProcessDelayedDeliveryService>(ComponentLifetime.SingleInstance, typeof (IDelayedDeliveryService));
            container.RegisterType<InProcessDeadLetterOffice>(ComponentLifetime.SingleInstance, typeof (IDeadLetterOffice));
            container.RegisterType<NamespaceCleanser>(ComponentLifetime.SingleInstance, typeof (INamespaceCleanser));
            container.RegisterType<UnsupportedLargeMessageBodyStore>(ComponentLifetime.SingleInstance, typeof (ILargeMessageBodyStore));
            container.RegisterType<InProcessTransport>(ComponentLifetime.SingleInstance, typeof (INimbusTransport));
        }

        public override IEnumerable<string> Validate()
        {
            yield break;
        }
    }
}