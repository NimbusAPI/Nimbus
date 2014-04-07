using System;
using System.IO;
using System.Linq;
using Nimbus.Configuration.Settings;
using Nimbus.HandlerFactories;
using Nimbus.Infrastructure;
using Nimbus.Infrastructure.BrokeredMessageServices.Compression;
using Nimbus.Infrastructure.BrokeredMessageServices.LargeMessages;

namespace Nimbus.Configuration
{
    public static class BusBuilderConfigurationExtensions
    {
        public static BusBuilderConfiguration WithConnectionString(this BusBuilderConfiguration configuration, string connectionString)
        {
            configuration.ConnectionString = new ConnectionStringSetting {Value = connectionString};
            return configuration;
        }

        public static BusBuilderConfiguration WithConnectionStringFromFile(this BusBuilderConfiguration configuration, string filename)
        {
            var connectionString = File.ReadAllText(filename).Trim();
            return configuration.WithConnectionString(connectionString);
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

        public static BusBuilderConfiguration WithMulticastEventHandlerFactory(this BusBuilderConfiguration configuration,
                                                                               IMulticastEventHandlerFactory multicastEventHandlerFactory)
        {
            configuration.MulticastEventHandlerFactory = multicastEventHandlerFactory;
            return configuration;
        }

        public static BusBuilderConfiguration WithCompetingEventHandlerFactory(this BusBuilderConfiguration configuration,
                                                                               ICompetingEventHandlerFactory competingEventHandlerFactory)
        {
            configuration.CompetingEventHandlerFactory = competingEventHandlerFactory;
            return configuration;
        }

        public static BusBuilderConfiguration WithCommandHandlerFactory(this BusBuilderConfiguration configuration, ICommandHandlerFactory commandHandlerFactory)
        {
            configuration.CommandHandlerFactory = commandHandlerFactory;
            return configuration;
        }

        public static BusBuilderConfiguration WithRequestHandlerFactory(this BusBuilderConfiguration configuration, IRequestHandlerFactory requestHandlerFactory)
        {
            configuration.RequestHandlerFactory = requestHandlerFactory;
            return configuration;
        }

        public static BusBuilderConfiguration WithMulticastRequestHandlerFactory(this BusBuilderConfiguration configuration, IMulticastRequestHandlerFactory requestHandlerFactory)
        {
            configuration.MulticastRequestHandlerFactory = requestHandlerFactory;
            return configuration;
        }

        public static BusBuilderConfiguration WithDefaultHandlerFactory(this BusBuilderConfiguration configuration, DefaultMessageHandlerFactory messageHandlerFactory)
        {
            configuration
                .WithCommandHandlerFactory(messageHandlerFactory)
                .WithRequestHandlerFactory(messageHandlerFactory)
                .WithMulticastRequestHandlerFactory(messageHandlerFactory)
                .WithCompetingEventHandlerFactory(messageHandlerFactory)
                .WithMulticastEventHandlerFactory(messageHandlerFactory);
            return configuration;
        }

        public static BusBuilderConfiguration WithTypesFrom(this BusBuilderConfiguration configuration, ITypeProvider typeProvider)
        {
            typeProvider.Verify();

            configuration.CommandHandlerTypes = new CommandHandlerTypesSetting {Value = typeProvider.CommandHandlerTypes.ToArray()};
            configuration.CommandTypes = new CommandTypesSetting {Value = typeProvider.CommandTypes.ToArray()};

            configuration.RequestHandlerTypes = new RequestHandlerTypesSetting {Value = typeProvider.RequestHandlerTypes.ToArray()};
            configuration.RequestTypes = new RequestTypesSetting {Value = typeProvider.RequestTypes.ToArray()};

            configuration.MulticastEventHandlerTypes = new MulticastEventHandlerTypesSetting {Value = typeProvider.MulticastEventHandlerTypes.ToArray()};
            configuration.CompetingEventHandlerTypes = new CompetingEventHandlerTypesSetting {Value = typeProvider.CompetingEventHandlerTypes.ToArray()};
            configuration.EventTypes = new EventTypesSetting {Value = typeProvider.EventTypes.ToArray()};

            return configuration;
        }

        public static BusBuilderConfiguration WithLargeBodyMessageStore(this BusBuilderConfiguration configuration, IMessageBodyStore messageBodyStore)
        {
            configuration.LargeMessageBodyStore = messageBodyStore;
            return configuration;
        }

        public static BusBuilderConfiguration WithServerConnectionCount(this BusBuilderConfiguration configuration, int serverConnectionCount)
        {
            configuration.ServerConnectionCount = new ServerConnectionCountSetting {Value = serverConnectionCount};
            return configuration;
        }

        public static BusBuilderConfiguration WithDefaultTimeout(this BusBuilderConfiguration configuration, TimeSpan defaultTimeout)
        {
            configuration.DefaultTimeout = new DefaultTimeoutSetting {Value = defaultTimeout};
            return configuration;
        }

        public static BusBuilderConfiguration WithDefaultMessageLockDuration(this BusBuilderConfiguration configuration, TimeSpan defaultLockDuration)
        {
            configuration.DefaultMessageLockDuration = new DefaultMessageLockDurationSetting {Value = defaultLockDuration};
            return configuration;
        }

        public static BusBuilderConfiguration WithDefaultConcurrentHandlerLimit(this BusBuilderConfiguration configuration, int defaultConcurrentHandlerLimit)
        {
            configuration.DefaultConcurrentHandlerLimit = new ConcurrentHandlerLimitSetting {Value = defaultConcurrentHandlerLimit};
            return configuration;
        }

        public static BusBuilderConfiguration WithMaxDeliveryAttempts(this BusBuilderConfiguration configuration, int maxDeliveryAttempts)
        {
            if (maxDeliveryAttempts < 1) throw new ArgumentOutOfRangeException("maxDeliveryAttempts", "You must attempt to deliver a message at least once.");

            configuration.MaxDeliveryAttempts = new MaxDeliveryAttemptSetting {Value = maxDeliveryAttempts};
            return configuration;
        }

        public static BusBuilderConfiguration WithLogger(this BusBuilderConfiguration configuration, ILogger logger)
        {
            configuration.Logger = logger;
            return configuration;
        }

        public static BusBuilderConfiguration WithSerializer(this BusBuilderConfiguration configuration, ISerializer serializer)
        {
            configuration.Serializer = serializer;
            return configuration;
        }

        public static BusBuilderConfiguration WithCompressor(this BusBuilderConfiguration configuration, ICompressor compressor)
        {
            configuration.Compressor = compressor;
            return configuration;
        }

        public static BusBuilderConfiguration WithDeflateCompressor(this BusBuilderConfiguration configuration)
        {
            return configuration.WithCompressor(new DeflateCompressor());
        }

        public static BusBuilderConfiguration WithGzipCompressor(this BusBuilderConfiguration configuration)
        {
            return configuration.WithCompressor(new GzipCompressor());
        }

        public static BusBuilderConfiguration WithDebugOptions(this BusBuilderConfiguration configuration,
                                                               Func<BusDebuggingConfiguration, BusDebuggingConfiguration> debugConfiguration)
        {
            debugConfiguration(configuration.Debugging);
            return configuration;
        }
    }
}