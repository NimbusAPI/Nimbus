using Nimbus.Configuration.Settings;
using Nimbus.Infrastructure.BrokeredMessageServices.LargeMessages;

namespace Nimbus.Configuration
{
    public static class BusBuilderLargeMessageConfigurationExtensions
    {
        public static BusBuilderConfiguration WithLargeMessageBodyStore(this BusBuilderConfiguration configuration, ILargeMessageBodyStore largeMessageBodyStore)
        {
            configuration.LargeMessageBodyStore = largeMessageBodyStore;
            return configuration;
        }

        public static BusBuilderConfiguration WithMaxSmallMessageSize(this BusBuilderConfiguration configuration, int messageSize)
        {
            configuration.MaxSmallMessageSize = new MaxSmallMessageSizeSetting { Value = messageSize };
            return configuration;
        }
        
        public static BusBuilderConfiguration WithMaxLargeMessageSize(this BusBuilderConfiguration configuration, int messageSize)
        {
            configuration.MaxLargeMessageSize = new MaxLargeMessageSizeSetting { Value = messageSize };
            return configuration;
        }
    }
}