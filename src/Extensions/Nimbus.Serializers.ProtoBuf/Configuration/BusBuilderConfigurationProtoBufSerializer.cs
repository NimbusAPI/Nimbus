using Nimbus.Serializers.ProtoBuf;

// ReSharper disable once CheckNamespace
namespace Nimbus.Configuration
{
    public static class BusBuilderConfigurationProtoBufSerializer
    {
        public static BusBuilderConfiguration WithProtoBufSerializer(this BusBuilderConfiguration configuration)
        {
            return configuration.WithSerializer(new ProtoBufSerializer());
        }
    }
}
