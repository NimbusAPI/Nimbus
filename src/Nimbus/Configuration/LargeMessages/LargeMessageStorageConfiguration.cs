using Nimbus.Configuration.LargeMessages.Settings;
using Nimbus.Configuration.PoorMansIocContainer;
using Nimbus.Infrastructure.BrokeredMessageServices.LargeMessages;

namespace Nimbus.Configuration.LargeMessages
{
    //FIXME abstract this in the same way as we're doing transport configuration
    public class LargeMessageStorageConfiguration : INimbusConfiguration
    {
        internal ILargeMessageBodyStore LargeMessageBodyStore { get; set; }
        internal MaxSmallMessageSizeSetting MaxSmallMessageSize { get; set; }
        internal MaxLargeMessageSizeSetting MaxLargeMessageSize { get; set; }

        public LargeMessageStorageConfiguration()
        {
            LargeMessageBodyStore = new UnsupportedLargeMessageBodyStore();
        }

        public void Register(PoorMansIoC container)
        {
        }
    }
}