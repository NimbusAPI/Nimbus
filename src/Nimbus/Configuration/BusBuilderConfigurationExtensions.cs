using System;
using System.Linq;
using Nimbus.Configuration.Settings;
using Nimbus.InfrastructureContracts;

namespace Nimbus.Configuration
{
    public static class BusBuilderConfigurationExtensions
    {
        public static BusBuilderConfiguration WithConnectionString(this BusBuilderConfiguration configuration, string connectionString)
        {
            configuration.ConnectionString = new ConnectionStringSetting {Value = connectionString};
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

        public static BusBuilderConfiguration WithMulticastEventBroker(this BusBuilderConfiguration configuration, IMulticastEventBroker multicastEventBroker)
        {
            configuration.MulticastEventBroker = multicastEventBroker;
            return configuration;
        }

        public static BusBuilderConfiguration WithCompetingEventBroker(this BusBuilderConfiguration configuration, ICompetingEventBroker competingEventBroker)
        {
            configuration.CompetingEventBroker = competingEventBroker;
            return configuration;
        }

        public static BusBuilderConfiguration WithCommandBroker(this BusBuilderConfiguration configuration, ICommandBroker commandBroker)
        {
            configuration.CommandBroker = commandBroker;
            return configuration;
        }

        public static BusBuilderConfiguration WithRequestBroker(this BusBuilderConfiguration configuration, IRequestBroker requestBroker)
        {
            configuration.RequestBroker = requestBroker;
            return configuration;
        }

        public static BusBuilderConfiguration WithMulticastRequestBroker(this BusBuilderConfiguration configuration, IMulticastRequestBroker requestBroker)
        {
            configuration.MulticastRequestBroker = requestBroker;
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

        public static BusBuilderConfiguration WithDefaultTimeout(this BusBuilderConfiguration configuration, TimeSpan defaultTimeout)
        {
            configuration.DefaultTimeout = new DefaultTimeoutSetting {Value = defaultTimeout};
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

        public static BusBuilderConfiguration WithDebugOptions(this BusBuilderConfiguration configuration,
                                                               Func<BusDebuggingConfiguration, BusDebuggingConfiguration> debugConfiguration)
        {
            debugConfiguration(configuration.Debugging);
            return configuration;
        }
    }
}