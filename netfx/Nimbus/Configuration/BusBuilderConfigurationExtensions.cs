using System;
using Nimbus.Configuration.Debug;
using Nimbus.Configuration.Settings;
using Nimbus.Configuration.Transport;
using Nimbus.DependencyResolution;
using Nimbus.Infrastructure.Compression;
using Nimbus.Infrastructure.DependencyResolution;
using Nimbus.Infrastructure.Logging;
using Nimbus.Infrastructure.Routing;
using Nimbus.Routing;

namespace Nimbus.Configuration
{
    [Obsolete("We should be able to inline these now.")]
    public static class BusBuilderConfigurationExtensions
    {
        public static BusBuilderConfiguration WithDefaults(this BusBuilderConfiguration configuration, ITypeProvider typeProvider)
        {
            return configuration
                .WithTypesFrom(typeProvider)
                .WithDependencyResolver(new DependencyResolver(typeProvider))
                .WithRouter(new DestinationPerMessageTypeRouter())
                .WithCompressor(new NullCompressor())
                .WithLogger(new NullLogger())
                ;
        }

        public static BusBuilderConfiguration WithTransport(this BusBuilderConfiguration configuration, TransportConfiguration transportConfiguration)
        {
            configuration.Transport = transportConfiguration;
            return configuration;
        }

        /// <summary>
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="applicationName">
        ///     This should be unique for your application (e.g. Foo.exe) but should be the same across
        ///     all instances of your application.
        /// </param>
        /// <param name="instanceName">
        ///     This should be unique across ALL instances of your application. Use your hostname if you're
        ///     stuck.
        /// </param>
        /// <returns></returns>
        public static BusBuilderConfiguration WithNames(this BusBuilderConfiguration configuration, string applicationName, string instanceName)
        {
            configuration.ApplicationName = new ApplicationNameSetting {Value = applicationName};
            configuration.InstanceName = new InstanceNameSetting {Value = instanceName};
            return configuration;
        }

        public static BusBuilderConfiguration WithGlobalPrefix(this BusBuilderConfiguration configuration, string globalPrefix)
        {
            configuration.GlobalPrefix = new GlobalPrefixSetting { Value = globalPrefix };
            return configuration;
        }

        public static BusBuilderConfiguration WithTypesFrom(this BusBuilderConfiguration configuration, ITypeProvider typeProvider)
        {
            configuration.TypeProvider = typeProvider;
            return configuration;
        }

        public static BusBuilderConfiguration WithDependencyResolver(this BusBuilderConfiguration configuration, IDependencyResolver dependencyResolver)
        {
            configuration.DependencyResolver = dependencyResolver;
            return configuration;
        }

        public static BusBuilderConfiguration WithRouter(this BusBuilderConfiguration configuration, IRouter router)
        {
            configuration.Router = router;
            return configuration;
        }

        public static BusBuilderConfiguration WithDeliveryRetryStrategy(this BusBuilderConfiguration configuration, IDeliveryRetryStrategy deliveryRetryStrategy)
        {
            configuration.DeliveryRetryStrategy = deliveryRetryStrategy;
            return configuration;
        }

        public static BusBuilderConfiguration WithDefaultTimeout(this BusBuilderConfiguration configuration, TimeSpan defaultTimeout)
        {
            configuration.DefaultTimeout = new DefaultTimeoutSetting {Value = defaultTimeout};
            return configuration;
        }

        public static BusBuilderConfiguration WithDefaultConcurrentHandlerLimit(this BusBuilderConfiguration configuration, int defaultConcurrentHandlerLimit)
        {
            configuration.ConcurrentHandlerLimit = new ConcurrentHandlerLimitSetting {Value = defaultConcurrentHandlerLimit};
            return configuration;
        }

        public static BusBuilderConfiguration WithMaxDeliveryAttempts(this BusBuilderConfiguration configuration, int maxDeliveryAttempts)
        {
            configuration.MaxDeliveryAttempts = new MaxDeliveryAttemptSetting {Value = maxDeliveryAttempts};
            return configuration;
        }

        public static BusBuilderConfiguration WithDefaultMessageTimeToLive(this BusBuilderConfiguration configuration, TimeSpan timeToLive)
        {
            configuration.DefaultMessageTimeToLive = new DefaultMessageTimeToLiveSetting {Value = timeToLive};
            return configuration;
        }

        public static BusBuilderConfiguration WithAutoDeleteOnIdle(this BusBuilderConfiguration configuration, TimeSpan autoDeleteOnIdle)
        {
            configuration.AutoDeleteOnIdle = new AutoDeleteOnIdleSetting {Value = autoDeleteOnIdle};
            return configuration;
        }

        public static BusBuilderConfiguration WithEnableDeadLetteringOnMessageExpiration(this BusBuilderConfiguration configuration, bool enableDeadLettering)
        {
            configuration.EnableDeadLetteringOnMessageExpiration = new EnableDeadLetteringOnMessageExpirationSetting {Value = enableDeadLettering};
            return configuration;
        }

        public static BusBuilderConfiguration WithHeartbeatInterval(this BusBuilderConfiguration configuration, TimeSpan heartbeatInterval)
        {
            configuration.HeartbeatInterval = new HeartbeatIntervalSetting {Value = heartbeatInterval};
            return configuration;
        }

        public static BusBuilderConfiguration WithLogger(this BusBuilderConfiguration configuration, ILogger logger)
        {
            configuration.Logger = logger;
            return configuration;
        }

        public static BusBuilderConfiguration WithGlobalInboundInterceptorTypes(this BusBuilderConfiguration configuration, params Type[] interceptorTypes)
        {
            configuration.GlobalInboundInterceptorTypes = new GlobalInboundInterceptorTypesSetting {Value = interceptorTypes};
            return configuration;
        }

        public static BusBuilderConfiguration WithGlobalOutboundInterceptorTypes(this BusBuilderConfiguration configuration, params Type[] interceptorTypes)
        {
            configuration.GlobalOutboundInterceptorTypes = new GlobalOutboundInterceptorTypesSetting {Value = interceptorTypes};
            return configuration;
        }

        public static BusBuilderConfiguration WithDebugOptions(this BusBuilderConfiguration configuration,
                                                               Func<DebugConfiguration, DebugConfiguration> debugConfiguration)
        {
            debugConfiguration(configuration.Debug);
            return configuration;
        }
    }
}