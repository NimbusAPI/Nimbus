using System;
using System.Collections.Generic;
using Nimbus.Configuration;
using Nimbus.Configuration.PoorMansIocContainer;
using Nimbus.Configuration.Transport;
using Nimbus.Infrastructure;
using Nimbus.Infrastructure.DependencyResolution;
using Nimbus.Infrastructure.LargeMessages;
using Nimbus.InfrastructureContracts;
using Nimbus.Transports.AMQP.ConnectionManagement;
using Nimbus.Transports.AMQP.DelayedDelivery;
using Nimbus.Transports.AMQP.MessageConversion;
using Nimbus.Transports.AMQP.MessageSendersAndReceivers;
using Nimbus.Transports.AMQP.QueueManagement;

namespace Nimbus.Transports.AMQP
{
    public class AMQPTransportConfiguration : TransportConfiguration
    {
        internal string BrokerUri { get; private set; }
        internal string Username { get; private set; }
        internal string Password { get; private set; }
        internal int ConnectionPoolSize { get; private set; } = 10;
        internal string[] FailoverUris { get; private set; } = new string[0];
        internal string ClientId { get; private set; }

        public AMQPTransportConfiguration WithBrokerUri(string brokerUri)
        {
            BrokerUri = brokerUri;
            return this;
        }

        public AMQPTransportConfiguration WithCredentials(string username, string password)
        {
            Username = username;
            Password = password;
            return this;
        }

        public AMQPTransportConfiguration WithConnectionPoolSize(int poolSize)
        {
            if (poolSize < 1) throw new ArgumentException("Pool size must be at least 1", nameof(poolSize));
            ConnectionPoolSize = poolSize;
            return this;
        }

        public AMQPTransportConfiguration WithFailover(params string[] failoverUris)
        {
            FailoverUris = failoverUris ?? new string[0];
            return this;
        }

        public AMQPTransportConfiguration WithClientId(string clientId)
        {
            ClientId = clientId;
            return this;
        }

        public AMQPTransportConfiguration WithArtemisDefaults()
        {
            return this;
        }

        protected override void RegisterComponents(PoorMansIoC container)
        {
            container.Register(c => this, ComponentLifetime.SingleInstance);
            // Register configuration
            container.RegisterType<AMQPTransport>(ComponentLifetime.SingleInstance, typeof (INimbusTransport));
            // Register connection management
            container.RegisterType<NmsConnectionFactory>(ComponentLifetime.SingleInstance);
            container.RegisterType<NmsConnectionPool>(ComponentLifetime.SingleInstance);

            // Register message conversion
            container.RegisterType<NmsMessageFactory>(ComponentLifetime.SingleInstance, typeof(INmsMessageFactory));

            // Register queue management
            container.RegisterType<AMQPQueueManager>(ComponentLifetime.SingleInstance, typeof(IQueueManager));

            // Register senders and receivers
            container.RegisterType<AMQPQueueSender>(ComponentLifetime.InstancePerDependency);
            container.RegisterType<AMQPQueueReceiver>(ComponentLifetime.InstancePerDependency);
            container.RegisterType<AMQPTopicSender>(ComponentLifetime.InstancePerDependency);
            container.RegisterType<AMQPTopicReceiver>(ComponentLifetime.InstancePerDependency);

            // Register transport-specific services
            //container.RegisterType<AMQPDelayedDeliveryService>(ComponentLifetime.SingleInstance, typeof(IDelayedDeliveryService));
            container.Register(f =>
                               {
                                   // Delay headers?
                                   return new AMQPDelayedDeliveryService(container.Resolve<INimbusTransport>(), container.Resolve<ILogger>());
                               }, ComponentLifetime.SingleInstance, typeof(IDelayedDeliveryService));
            
            container.RegisterType<AMQPDeadLetterOffice>(ComponentLifetime.SingleInstance, typeof(IDeadLetterOffice));
            container.RegisterType<AMQPNamespaceCleanser>(ComponentLifetime.SingleInstance, typeof(INamespaceCleanser));
            container.RegisterType<UnsupportedLargeMessageBodyStore>(ComponentLifetime.SingleInstance, typeof(ILargeMessageBodyStore));

            // Register the transport itself
            container.RegisterType<AMQPTransport>(ComponentLifetime.SingleInstance, typeof(INimbusTransport));
        }

        public override IEnumerable<string> Validate()
        {
            if (string.IsNullOrWhiteSpace(BrokerUri))
                yield return "Broker URI must be specified. Use WithBrokerUri() method.";
        }
    }
}
