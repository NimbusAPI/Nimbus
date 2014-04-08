using Nimbus.Configuration.LargeMessages;
using Nimbus.Configuration.LargeMessages.Settings;

namespace Nimbus.Configuration
{
    public static class BusBuilderLargeMessageConfigurationExtensions
    {
        public static LargeMessageStorageConfiguration WithMaxSmallMessageSize(this LargeMessageStorageConfiguration configuration, int messageSize)
        {
            configuration.MaxSmallMessageSize = new MaxSmallMessageSizeSetting {Value = messageSize};
            return configuration;
        }

        public static LargeMessageStorageConfiguration WithMaxLargeMessageSize(this LargeMessageStorageConfiguration configuration, int messageSize)
        {
            configuration.MaxLargeMessageSize = new MaxLargeMessageSizeSetting {Value = messageSize};
            return configuration;
        }
    }
}