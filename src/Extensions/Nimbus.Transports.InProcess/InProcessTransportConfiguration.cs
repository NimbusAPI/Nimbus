using System.Collections.Generic;
using Nimbus.Configuration;
using Nimbus.Configuration.PoorMansIocContainer;
using Nimbus.Configuration.Transport;
using Nimbus.DevelopmentStubs;
using Nimbus.Infrastructure;
using Nimbus.Transports.InProcess.MessageSendersAndReceivers;

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
            container.RegisterType<InProcessDelayedDeliveryService>(ComponentLifetime.SingleInstance, typeof (IDelayedDeliveryService));
            container.RegisterType<InProcessDeadLetterOffice>(ComponentLifetime.SingleInstance, typeof (IDeadLetterOffice));
            container.RegisterType<StubNamespaceCleanser>(ComponentLifetime.SingleInstance, typeof (INamespaceCleanser));
            container.RegisterType<InProcessTransport>(ComponentLifetime.SingleInstance, typeof (INimbusTransport));
        }

        public override IEnumerable<string> Validate()
        {
            yield break;
        }
    }
}