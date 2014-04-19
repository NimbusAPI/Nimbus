using Nimbus.Configuration.LargeMessages.Settings;

namespace Nimbus.Configuration.LargeMessages
{
    public class LargeMessageStorageConfiguration : INimbusConfiguration
    {
        internal ILargeMessageBodyStore LargeMessageBodyStore { get; set; }
        internal MaxSmallMessageSizeSetting MaxSmallMessageSize { get; set; }
        internal MaxLargeMessageSizeSetting MaxLargeMessageSize { get; set; }
    }
}