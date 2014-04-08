using Nimbus.Configuration.LargeMessages.Settings;
using Nimbus.Infrastructure.BrokeredMessageServices.LargeMessages;

namespace Nimbus.Configuration.LargeMessages
{
    public abstract class LargeMessageStorageConfiguration : ILargeMessageBodyConfiguration
    {
        public abstract ILargeMessageBodyStore LargeMessageBodyStore { get; }
        public MaxSmallMessageSizeSetting MaxSmallMessageSize { get; set; }
        public MaxLargeMessageSizeSetting MaxLargeMessageSize { get; set; }
    }
}