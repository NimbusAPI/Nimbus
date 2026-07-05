using System.Collections.Generic;
using Nimbus.Configuration;
using Nimbus.Configuration.PoorMansIocContainer;
using Nimbus.Configuration.Transport;
using Nimbus.Infrastructure;
using Nimbus.Infrastructure.LargeMessages;
using Nimbus.InfrastructureContracts;
using Nimbus.Transports.RabbitMQ.ConnectionManagement;
using Nimbus.Transports.RabbitMQ.DeadLetter;
using Nimbus.Transports.RabbitMQ.DelayedDelivery;
using Nimbus.Transports.RabbitMQ.MessageConversion;
using Nimbus.Transports.RabbitMQ.MessageSendersAndReceivers;
using Nimbus.Transports.RabbitMQ.QueueManagement;

namespace Nimbus.Transports.RabbitMQ
{
    public class RabbitMqTransportConfiguration : TransportConfiguration
    {
        internal string Host { get; private set; } = "localhost";
        internal int Port { get; private set; } = 5672;
        internal int? ManagementPort { get; private set; }
        internal string Username { get; private set; } = "guest";
        internal string Password { get; private set; } = "guest";
        internal string VirtualHost { get; private set; } = "/";

        internal string ManagementUri =>
            ManagementPort.HasValue
                ? $"http://{Host}:{ManagementPort.Value}"
                : null;

        public RabbitMqTransportConfiguration WithHost(string host)
        {
            Host = host;
            return this;
        }

        public RabbitMqTransportConfiguration WithPort(int port)
        {
            Port = port;
            return this;
        }

        public RabbitMqTransportConfiguration WithManagementPort(int managementPort)
        {
            ManagementPort = managementPort;
            return this;
        }

        public RabbitMqTransportConfiguration WithCredentials(string username, string password)
        {
            Username = username;
            Password = password;
            return this;
        }

        public RabbitMqTransportConfiguration WithVirtualHost(string virtualHost)
        {
            VirtualHost = virtualHost;
            return this;
        }

        protected override void RegisterComponents(PoorMansIoC container)
        {
            container.Register(c => this, ComponentLifetime.SingleInstance);

            container.RegisterType<RabbitMqConnectionManager>(ComponentLifetime.SingleInstance);
            container.RegisterType<RabbitMqMessageConverter>(ComponentLifetime.SingleInstance);

            container.RegisterType<RabbitMqQueueSender>(ComponentLifetime.InstancePerDependency);
            container.RegisterType<RabbitMqQueueReceiver>(ComponentLifetime.InstancePerDependency);
            container.RegisterType<RabbitMqTopicSender>(ComponentLifetime.InstancePerDependency);
            container.RegisterType<RabbitMqTopicReceiver>(ComponentLifetime.InstancePerDependency);

            container.Register(c => new RabbitMqDelayedDeliveryService(c.Resolve<INimbusTransport>(), c.Resolve<ILogger>()),
                               ComponentLifetime.SingleInstance, typeof(IDelayedDeliveryService));
            container.Register(c => new RabbitMqDeadLetterOffice(
                                   c.Resolve<INimbusTransport>(),
                                   c.Resolve<RabbitMqConnectionManager>(),
                                   c.Resolve<RabbitMqMessageConverter>(),
                                   c.Resolve<ILogger>()),
                               ComponentLifetime.SingleInstance, typeof(IDeadLetterOffice));
            container.Register(c => new RabbitMqNamespaceCleanser(c.Resolve<RabbitMqTransportConfiguration>(), c.Resolve<ILogger>()),
                               ComponentLifetime.SingleInstance, typeof(INamespaceCleanser));
            container.RegisterType<UnsupportedLargeMessageBodyStore>(ComponentLifetime.SingleInstance, typeof(ILargeMessageBodyStore));

            container.RegisterType<RabbitMqTransport>(ComponentLifetime.SingleInstance, typeof(INimbusTransport));
        }

        public override IEnumerable<string> Validate()
        {
            if (string.IsNullOrWhiteSpace(Host))
                yield return "Host must be specified. Use WithHost().";
        }
    }
}
