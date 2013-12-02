using System;
using System.Linq;
using Nimbus.InfrastructureContracts;

namespace Nimbus.Configuration
{
    public static class BusBuilderConfigurationExtensions
    {
        public static BusBuilderConfiguration WithConnectionString(this BusBuilderConfiguration configuration, string connectionString)
        {
            configuration.ConnectionString = connectionString;
            return configuration;
        }

        /// <summary>
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="applicationName">This should be unique for your application (e.g. Foo.exe) but should be the same across all instances of your application.</param>
        /// <param name="instanceName">This should be unique across ALL instances of your application. Use your hostname if you're stuck.</param>
        /// <returns></returns>
        public static BusBuilderConfiguration WithNames(this BusBuilderConfiguration configuration, string applicationName, string instanceName)
        {
            configuration.ApplicationName = applicationName;
            configuration.InstanceName = instanceName;
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
        
        public static BusBuilderConfiguration WithTimeoutBroker(this BusBuilderConfiguration configuration, ITimeoutBroker timeoutBroker)
        {
            configuration.TimeoutBroker = timeoutBroker;
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
            configuration.CommandHandlerTypes = typeProvider.CommandHandlerTypes.ToArray();
            configuration.CommandTypes = typeProvider.CommandTypes.ToArray();
            
            configuration.TimeoutHandlerTypes = typeProvider.TimeoutHandlerTypes.ToArray();
            configuration.TimeoutTypes = typeProvider.TimeoutTypes.ToArray();

            configuration.RequestHandlerTypes = typeProvider.RequestHandlerTypes.ToArray();
            configuration.RequestTypes = typeProvider.RequestTypes.ToArray();

            configuration.MulticastEventHandlerTypes = typeProvider.MulticastEventHandlerTypes.ToArray();
            configuration.CompetingEventHandlerTypes = typeProvider.CompetingEventHandlerTypes.ToArray();
            configuration.EventTypes = typeProvider.EventTypes.ToArray();

            return configuration;
        }

        public static BusBuilderConfiguration WithDefaultTimeout(this BusBuilderConfiguration configuration, TimeSpan defaultTimeout)
        {
            configuration.DefaultTimeout = defaultTimeout;
            return configuration;
        }

        public static BusBuilderConfiguration WithMaxDeliveryAttempts(this BusBuilderConfiguration configuration, int maxDeliveryAttempts)
        {
            if (maxDeliveryAttempts < 1) throw new ArgumentOutOfRangeException("maxDeliveryAttempts", "You must attempt to deliver a message at least once.");

            configuration.MaxDeliveryAttempts = maxDeliveryAttempts;
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