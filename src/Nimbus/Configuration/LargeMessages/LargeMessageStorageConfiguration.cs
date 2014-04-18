using Nimbus.Configuration.LargeMessages.Settings;

namespace Nimbus.Configuration.LargeMessages
{
    public abstract class LargeMessageStorageConfiguration : ILargeMessageStorageConfiguration
    {
        public abstract ILargeMessageBodyStore LargeMessageBodyStore { get; }
        public MaxSmallMessageSizeSetting MaxSmallMessageSize { get; set; }
        public MaxLargeMessageSizeSetting MaxLargeMessageSize { get; set; }
    }
}