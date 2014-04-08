using Nimbus.Infrastructure;

namespace Nimbus.Configuration
{
    public static class BusBuilderSerializationConfigurationExtensions
    {
        public static BusBuilderConfiguration WithSerializer(this BusBuilderConfiguration configuration, ISerializer serializer)
        {
            configuration.Serializer = serializer;
            return configuration;
        }
    }
}