using Nimbus.Configuration.LargeMessages.Settings;
using Nimbus.Infrastructure.BrokeredMessageServices.LargeMessages;

namespace Nimbus.Configuration.LargeMessages
{
    internal class UnsupportedLargeMessageBodyConfiguration : ILargeMessageBodyConfiguration
    {
        public ILargeMessageBodyStore LargeMessageBodyStore { get; internal set; }
        public MaxSmallMessageSizeSetting MaxSmallMessageSize { get { return new MaxSmallMessageSizeSetting(); } }
        public MaxLargeMessageSizeSetting MaxLargeMessageSize { get { return new MaxLargeMessageSizeSetting(); } }

        public UnsupportedLargeMessageBodyConfiguration()
        {
            LargeMessageBodyStore = new UnsupportedLargeMessageBodyStore();
        }
    }
}