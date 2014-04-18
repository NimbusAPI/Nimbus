using Nimbus.Configuration.LargeMessages.Settings;

namespace Nimbus.Configuration.LargeMessages
{
    public interface ILargeMessageStorageConfiguration : INimbusConfiguration
    {
        ILargeMessageBodyStore LargeMessageBodyStore { get; }
        MaxSmallMessageSizeSetting MaxSmallMessageSize { get; }
        MaxLargeMessageSizeSetting MaxLargeMessageSize { get; }
    }
}