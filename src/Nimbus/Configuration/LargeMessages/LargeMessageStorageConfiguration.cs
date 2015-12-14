using Nimbus.Configuration.LargeMessages.Settings;
using Nimbus.Infrastructure.BrokeredMessageServices.LargeMessages;

namespace Nimbus.Configuration.LargeMessages
{
    public class LargeMessageStorageConfiguration : INimbusConfiguration
    {
        internal ILargeMessageBodyStore LargeMessageBodyStore { get; set; }
        internal MaxSmallMessageSizeSetting MaxSmallMessageSize { get; set; }
        internal MaxLargeMessageSizeSetting MaxLargeMessageSize { get; set; }

        public LargeMessageStorageConfiguration()
        {
            LargeMessageBodyStore = new UnsupportedLargeMessageBodyStore();
        }
    }
}