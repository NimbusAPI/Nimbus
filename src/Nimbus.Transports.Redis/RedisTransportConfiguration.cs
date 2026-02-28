using System;
using System.Collections.Generic;
using Nimbus.ConcurrentCollections;
using Nimbus.Configuration;
using Nimbus.Configuration.PoorMansIocContainer;
using Nimbus.Configuration.Settings;
using Nimbus.Configuration.Transport;
using Nimbus.DevelopmentStubs;
using Nimbus.Infrastructure;
using Nimbus.Infrastructure.LargeMessages;
using Nimbus.InfrastructureContracts;
using Nimbus.Transports.Redis.Configuration;
using Nimbus.Transports.Redis.ConnectionManagement;
using Nimbus.Transports.Redis.DeadLetterOffice;
using Nimbus.Transports.Redis.DelayedDelivery;
using Nimbus.Transports.Redis.MessageSendersAndReceivers;
using Nimbus.Transports.Redis.QueueManagement;
using StackExchange.Redis;

namespace Nimbus.Transports.Redis
{
    public class RedisTransportConfiguration : TransportConfiguration
    {
        internal RedisConnectionString ConnectionString { get; set; }
        internal ServerConnectionCountSetting ServerConnectionCount { get; set; } = new ServerConnectionCountSetting();

        public RedisTransportConfiguration WithConnectionString(string connectionString)
        {
            ConnectionString = new RedisConnectionString {Value = connectionString};
            return this;
        }

        public RedisTransportConfiguration WithServerConnectionCount(int connectionCount)
        {
            ServerConnectionCount = new ServerConnectionCountSetting {Value = connectionCount};
            return this;
        }

        protected override void RegisterComponents(PoorMansIoC container)
        {
            container.RegisterType<ConnectionMultiplexerFactory>(ComponentLifetime.SingleInstance);
            container.Register(c => new RoundRobin<ConnectionMultiplexer>(c.Resolve<ServerConnectionCountSetting>(),
                                                                          () => c.Resolve<ConnectionMultiplexerFactory>().Create(),
                                                                          multiplexer => false,
                                                                          multiplexer => multiplexer.Dispose()),
                               ComponentLifetime.SingleInstance);
            container.Register<Func<ConnectionMultiplexer>>(c => c.Resolve<RoundRobin<ConnectionMultiplexer>>().GetNext, ComponentLifetime.InstancePerDependency);
            container.Register<Func<IDatabase>>(c => () => c.Resolve<Func<ConnectionMultiplexer>>()().GetDatabase(), ComponentLifetime.InstancePerDependency);
            container.RegisterType<RedisMessageSender>(ComponentLifetime.InstancePerDependency);
            container.RegisterType<RedisMessageReceiver>(ComponentLifetime.InstancePerDependency);
            container.RegisterType<RedisTopicSender>(ComponentLifetime.InstancePerDependency);
            container.RegisterType<RedisSubscriptionReceiver>(ComponentLifetime.InstancePerDependency);

            container.RegisterType<RedisDelayedDeliveryService>(ComponentLifetime.SingleInstance, typeof (IDelayedDeliveryService));
            container.RegisterType<RedisDeadLetterOffice>(ComponentLifetime.SingleInstance, typeof (IDeadLetterOffice));
            container.RegisterType<NamespaceCleanser>(ComponentLifetime.SingleInstance, typeof (INamespaceCleanser));
            container.RegisterType<UnsupportedLargeMessageBodyStore>(ComponentLifetime.SingleInstance, typeof (ILargeMessageBodyStore));

            container.RegisterType<RedisTransport>(ComponentLifetime.SingleInstance, typeof (INimbusTransport));
        }

        public override IEnumerable<string> Validate()
        {
            yield break;
        }
    }
}