using System.Collections.Generic;
using Nimbus.Configuration.Debug;
using Nimbus.Configuration.PoorMansIocContainer;
using Nimbus.Configuration.Settings;
using Nimbus.Configuration.Transport;
using Nimbus.DependencyResolution;
using Nimbus.DevelopmentStubs;
using Nimbus.Infrastructure;
using Nimbus.Infrastructure.Commands;
using Nimbus.Infrastructure.Compression;
using Nimbus.Infrastructure.DependencyResolution;
using Nimbus.Infrastructure.Dispatching;
using Nimbus.Infrastructure.Events;
using Nimbus.Infrastructure.Filtering;
using Nimbus.Infrastructure.Heartbeat;
using Nimbus.Infrastructure.Logging;
using Nimbus.Infrastructure.MessageSendersAndReceivers;
using Nimbus.Infrastructure.PropertyInjection;
using Nimbus.Infrastructure.RequestResponse;
using Nimbus.Infrastructure.Retries;
using Nimbus.Infrastructure.Routing;
using Nimbus.Infrastructure.Serialization;
using Nimbus.Interceptors.Inbound;
using Nimbus.Interceptors.Outbound;
using Nimbus.Routing;

namespace Nimbus.Configuration
{
    public class BusBuilderConfiguration : INimbusConfiguration
    {
        internal TransportConfiguration Transport { get; set; }
        internal DebugConfiguration Debug { get; set; } = new DebugConfiguration();

        internal ITypeProvider TypeProvider { get; set; }
        internal IDependencyResolver DependencyResolver { get; set; }
        internal ILogger Logger { get; set; } = new NullLogger();
        internal ISerializer Serializer { get; set; }
        internal ICompressor Compressor { get; set; } = new NullCompressor();
        internal IRouter Router { get; set; } = new DestinationPerMessageTypeRouter();
        internal IDeliveryRetryStrategy DeliveryRetryStrategy { get; set; } = new StubDeliveryRetryStrategy();
        internal IPathFactory PathFactory { get; set; } = Infrastructure.PathFactory.CreateWithNoPrefix();

        internal ApplicationNameSetting ApplicationName { get; set; }
        internal InstanceNameSetting InstanceName { get; set; }
        internal DefaultTimeoutSetting DefaultTimeout { get; set; } = new DefaultTimeoutSetting();
        internal MaxDeliveryAttemptSetting MaxDeliveryAttempts { get; set; } = new MaxDeliveryAttemptSetting();
        internal DefaultMessageTimeToLiveSetting DefaultMessageTimeToLive { get; set; } = new DefaultMessageTimeToLiveSetting();
        internal AutoDeleteOnIdleSetting AutoDeleteOnIdle { get; set; } = new AutoDeleteOnIdleSetting();
        internal EnableDeadLetteringOnMessageExpirationSetting EnableDeadLetteringOnMessageExpiration { get; set; } = new EnableDeadLetteringOnMessageExpirationSetting();
        internal HeartbeatIntervalSetting HeartbeatInterval { get; set; } = new HeartbeatIntervalSetting();
        internal GlobalInboundInterceptorTypesSetting GlobalInboundInterceptorTypes { get; set; } = new GlobalInboundInterceptorTypesSetting();
        internal GlobalOutboundInterceptorTypesSetting GlobalOutboundInterceptorTypes { get; set; } = new GlobalOutboundInterceptorTypesSetting();
        internal ConcurrentHandlerLimitSetting ConcurrentHandlerLimit { get; set; } = new ConcurrentHandlerLimitSetting();
        internal GlobalConcurrentHandlerLimitSetting GlobalConcurrentHandlerLimit { get; set; } = new GlobalConcurrentHandlerLimitSetting();

        public Bus Build()
        {
            if (TypeProvider != null)
            {
                if (Serializer == null)
                {
                    Serializer = new DataContractSerializer(TypeProvider);
                }

                if (DependencyResolver == null)
                {
                    DependencyResolver = new DependencyResolver(TypeProvider);
                }
            }

            return BusBuilder.Build(this);
        }

        public void RegisterWith(PoorMansIoC container)
        {
            container.Register(TypeProvider, typeof(ITypeProvider));
            container.Register(DependencyResolver, typeof(IDependencyResolver));
            container.Register(Logger, typeof(ILogger));
            container.Register(Serializer, typeof(ISerializer));
            container.Register(Compressor, typeof(ICompressor));
            container.Register(Router, typeof(IRouter));
            container.Register(DeliveryRetryStrategy, typeof(IDeliveryRetryStrategy));
            container.Register(PathFactory, typeof(IPathFactory));

            container.RegisterType<ReplyQueueNameSetting>(ComponentLifetime.SingleInstance);
            container.RegisterType<RequestResponseCorrelator>(ComponentLifetime.SingleInstance);
            container.RegisterType<CommandMessagePumpsFactory>(ComponentLifetime.SingleInstance);
            container.RegisterType<RequestMessagePumpsFactory>(ComponentLifetime.SingleInstance);
            container.RegisterType<ResponseMessagePumpFactory>(ComponentLifetime.SingleInstance);
            container.RegisterType<MulticastRequestMessagePumpsFactory>(ComponentLifetime.SingleInstance);
            container.RegisterType<MulticastEventMessagePumpsFactory>(ComponentLifetime.SingleInstance);
            container.RegisterType<CompetingEventMessagePumpsFactory>(ComponentLifetime.SingleInstance);
            container.RegisterType<SystemClock>(ComponentLifetime.SingleInstance, typeof(IClock));
            container.RegisterType<DispatchContextManager>(ComponentLifetime.SingleInstance, typeof(IDispatchContextManager));
            container.RegisterType<ResponseMessageDispatcher>(ComponentLifetime.SingleInstance);
            container.RegisterType<HandlerMapper>(ComponentLifetime.SingleInstance, typeof(IHandlerMapper));
            container.RegisterType<MessageDispatcherFactory>(ComponentLifetime.SingleInstance, typeof(IMessageDispatcherFactory));
            container.RegisterType<InboundInterceptorFactory>(ComponentLifetime.SingleInstance, typeof(IInboundInterceptorFactory));
            container.RegisterType<OutboundInterceptorFactory>(ComponentLifetime.SingleInstance, typeof(IOutboundInterceptorFactory));
            container.RegisterType<PropertyInjector>(ComponentLifetime.SingleInstance, typeof(IPropertyInjector), typeof(PropertyInjector));
            container.RegisterType<NimbusMessageFactory>(ComponentLifetime.SingleInstance, typeof(INimbusMessageFactory));
            container.RegisterType<BusCommandSender>(ComponentLifetime.SingleInstance, typeof(ICommandSender));
            container.RegisterType<BusRequestSender>(ComponentLifetime.SingleInstance, typeof(IRequestSender));
            container.RegisterType<BusMulticastRequestSender>(ComponentLifetime.SingleInstance, typeof(IMulticastRequestSender));
            container.RegisterType<BusEventSender>(ComponentLifetime.SingleInstance, typeof(IEventSender));
            container.RegisterType<KnownMessageTypeVerifier>(ComponentLifetime.SingleInstance, typeof(IKnownMessageTypeVerifier));
            container.RegisterType<Heartbeat>(ComponentLifetime.SingleInstance, typeof(IHeartbeat));
            container.RegisterType<Bus>(ComponentLifetime.SingleInstance);
            container.RegisterType<GlobalHandlerThrottle>(ComponentLifetime.SingleInstance, typeof(IGlobalHandlerThrottle));
            container.RegisterType<FilterConditionProvider>(ComponentLifetime.SingleInstance, typeof(IFilterConditionProvider));

            container.RegisterType<MessagePump>(ComponentLifetime.InstancePerDependency);
            container.RegisterType<DefaultRetry>(ComponentLifetime.InstancePerDependency, typeof(IRetry));
        }

        public IEnumerable<string> Validate()
        {
            if (ApplicationName == null) yield return "You must specify an application name.";
            if (InstanceName == null) yield return "You must specify an instance name.";

            if (Transport == null) yield return "You must specify a transport layer.";
            if (TypeProvider == null) yield return "You must specify a type provider.";
            if (DependencyResolver == null) yield return "You must specify a dependency resolver.";
            if (Logger == null) yield return "You must specify a logger.";
            if (Serializer == null) yield return "You must specify a serializer.";
            if (Compressor == null) yield return "You must specify a compressor.";
            if (Router == null) yield return "You must specify a router.";
            if (DeliveryRetryStrategy == null) yield return "You must specify a delivery retry strategy.";
        }
    }
}