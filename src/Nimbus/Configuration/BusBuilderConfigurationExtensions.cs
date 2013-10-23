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

        public static BusBuilderConfiguration WithInstanceName(this BusBuilderConfiguration configuration, string instanceName)
        {
            configuration.InstanceName = instanceName;
            return configuration;
        }

        public static BusBuilderConfiguration WithEventBroker(this BusBuilderConfiguration configuration, IEventBroker eventBroker)
        {
            configuration.EventBroker = eventBroker;
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

        public static BusBuilderConfiguration WithHandlerTypesFrom(this BusBuilderConfiguration configuration, ITypeProvider typeProvider)
        {
            configuration.CommandHandlerTypes = typeProvider.CommandHandlerTypes.ToArray();
            configuration.RequestHandlerTypes = typeProvider.RequestHandlerTypes.ToArray();
            configuration.EventHandlerTypes = typeProvider.EventHandlerTypes.ToArray();

            return configuration;
        }
    }
}