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
using Nimbus.Transports.Redis.ConnectionManagement;
using Nimbus.Transports.Redis.DeadLetterOffice;
using Nimbus.Transports.Redis.DelayedDelivery;
using Nimbus.Transports.Redis.MessageSendersAndReceivers;
using StackExchange.Redis;

namespace Nimbus.Transports.Redis
{
    public class RedisTransportConfiguration : TransportConfiguration
    {
        protected override void RegisterComponents(PoorMansIoC container)
        {
            container.Register(c => new RoundRobin<ConnectionMultiplexer>(c.Resolve<ServerConnectionCountSetting>(),
                                                                          () => c.Resolve<ConnectionMultiplexerFactory>().Create(),
                                                                          multiplexer => false,
                                                                          multiplexer => multiplexer.Dispose()),
                               ComponentLifetime.SingleInstance);
            container.Register<Func<ConnectionMultiplexer>>(c => c.Resolve<RoundRobin<ConnectionMultiplexer>>().GetNext, ComponentLifetime.InstancePerDependency);
            container.Register<Func<IDatabase>>(c => () => c.Resolve<ConnectionMultiplexer>().GetDatabase(), ComponentLifetime.InstancePerDependency);
            container.RegisterType<RedisMessageSender>(ComponentLifetime.InstancePerDependency);

            container.RegisterType<RedisDelayedDeliveryService>(ComponentLifetime.SingleInstance, typeof (IDelayedDeliveryService));
            container.RegisterType<RedisDeadLetterOffice>(ComponentLifetime.SingleInstance, typeof (IDeadLetterOffice));
            container.RegisterType<StubNamespaceCleanser>(ComponentLifetime.SingleInstance, typeof (INamespaceCleanser));
            container.RegisterType<UnsupportedLargeMessageBodyStore>(ComponentLifetime.SingleInstance, typeof (ILargeMessageBodyStore));

            container.RegisterType<RedisTransport>(ComponentLifetime.SingleInstance, typeof (INimbusTransport));
        }

        public override IEnumerable<string> Validate()
        {
            yield break;
        }
    }
}