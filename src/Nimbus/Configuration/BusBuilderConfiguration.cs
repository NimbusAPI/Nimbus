using System.Collections.Generic;
using Nimbus.Configuration.Debug;
using Nimbus.Configuration.PoorMansIocContainer;
using Nimbus.Configuration.Settings;
using Nimbus.Configuration.Transport;
using Nimbus.DependencyResolution;
using Nimbus.DevelopmentStubs;
using Nimbus.Infrastructure;
using Nimbus.Infrastructure.BrokeredMessageServices.Compression;
using Nimbus.Infrastructure.BrokeredMessageServices.Serialization;
using Nimbus.Infrastructure.Commands;
using Nimbus.Infrastructure.Dispatching;
using Nimbus.Infrastructure.Events;
using Nimbus.Infrastructure.Heartbeat;
using Nimbus.Infrastructure.Logging;
using Nimbus.Infrastructure.NimbusMessageServices;
using Nimbus.Infrastructure.PropertyInjection;
using Nimbus.Infrastructure.RequestResponse;
using Nimbus.Infrastructure.Routing;
using Nimbus.Interceptors.Inbound;
using Nimbus.Interceptors.Outbound;
using Nimbus.PoisonMessages;
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
        internal ConcurrentHandlerLimitSetting DefaultConcurrentHandlerLimit { get; set; } = new ConcurrentHandlerLimitSetting();

        public Bus Build()
        {
            if (Serializer == null && TypeProvider != null)
            {
                Serializer = new DataContractSerializer(TypeProvider);
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

            container.RegisterType<ReplyQueueNameSetting>(ComponentLifetime.SingleInstance);
            container.RegisterType<RequestResponseCorrelator>(ComponentLifetime.SingleInstance);
            container.RegisterType<CommandMessagePumpsFactory>(ComponentLifetime.SingleInstance);
            container.RegisterType<RequestMessagePumpsFactory>(ComponentLifetime.SingleInstance);
            container.RegisterType<ResponseMessagePumpFactory>(ComponentLifetime.SingleInstance);
            container.RegisterType<MulticastRequestMessagePumpsFactory>(ComponentLifetime.SingleInstance);
            container.RegisterType<MulticastEventMessagePumpsFactory>(ComponentLifetime.SingleInstance);
            container.RegisterType<CompetingEventMessagePumpsFactory>(ComponentLifetime.SingleInstance);
            container.RegisterType<SystemClock>(ComponentLifetime.SingleInstance, typeof (IClock));
            container.RegisterType<DispatchContextManager>(ComponentLifetime.SingleInstance, typeof (IDispatchContextManager));
            container.RegisterType<ResponseMessageDispatcher>(ComponentLifetime.SingleInstance);
            container.RegisterType<HandlerMapper>(ComponentLifetime.SingleInstance, typeof (IHandlerMapper));
            container.RegisterType<MessageDispatcherFactory>(ComponentLifetime.SingleInstance, typeof (IMessageDispatcherFactory));
            container.RegisterType<InboundInterceptorFactory>(ComponentLifetime.SingleInstance, typeof (IInboundInterceptorFactory));
            container.RegisterType<OutboundInterceptorFactory>(ComponentLifetime.SingleInstance, typeof (IOutboundInterceptorFactory));
            container.RegisterType<PropertyInjector>(ComponentLifetime.SingleInstance, typeof (IPropertyInjector));
            container.RegisterType<NimbusMessageFactory>(ComponentLifetime.SingleInstance, typeof (INimbusMessageFactory));
            container.RegisterType<BusCommandSender>(ComponentLifetime.SingleInstance, typeof (ICommandSender));
            container.RegisterType<BusRequestSender>(ComponentLifetime.SingleInstance, typeof (IRequestSender));
            container.RegisterType<BusMulticastRequestSender>(ComponentLifetime.SingleInstance, typeof (IMulticastRequestSender));
            container.RegisterType<BusEventSender>(ComponentLifetime.SingleInstance, typeof (IEventSender));
            container.RegisterType<KnownMessageTypeVerifier>(ComponentLifetime.SingleInstance, typeof (IKnownMessageTypeVerifier));
            container.RegisterType<Heartbeat>(ComponentLifetime.SingleInstance, typeof (IHeartbeat));
            container.RegisterType<Bus>(ComponentLifetime.SingleInstance);
            container.RegisterType<MessagePump>(ComponentLifetime.InstancePerDependency);

            #region To be fixed

            //FIXME these are either stubs that are yet to be implemented or obsolete components to be removed
            container.RegisterType<DeadLetterQueues>(ComponentLifetime.SingleInstance, typeof (DeadLetterQueues), typeof (IDeadLetterQueues));
            //container.RegisterType<DeadLetterQueue>(ComponentLifetime.SingleInstance, typeof (IDeadLetterQueue));
            container.RegisterType<StubDeadLetterOffice>(ComponentLifetime.SingleInstance, typeof (IDeadLetterOffice));
            container.RegisterType<StubDeliveryRetryStrategy>(ComponentLifetime.SingleInstance, typeof (IDeliveryRetryStrategy));

            #endregion
        }

        public IEnumerable<string> Validate()
        {
            if (Transport == null) yield return "You must specify a transport later.";
        }
    }
}