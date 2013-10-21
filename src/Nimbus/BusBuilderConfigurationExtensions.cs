using System;

namespace Nimbus
{
    public static class BusBuilderConfigurationExtensions
    {
         public static BusBuilder WithConnectionString(this BusBuilder builder, string connectionString)
         {
             builder.ConnectionString = connectionString;
             return builder;
         }

        public static BusBuilder WithInstanceName(this BusBuilder builder, string instanceName)
        {
            builder.InstanceName = instanceName;
            return builder;
        }

        public static BusBuilder WithEventBroker(this BusBuilder builder, Func<IEventBroker> brokerFactory)
        {
            return builder;
        }
    }
}