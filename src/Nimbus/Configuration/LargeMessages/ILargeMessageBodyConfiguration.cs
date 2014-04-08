using Nimbus.Configuration.LargeMessages.Settings;
using Nimbus.Infrastructure.BrokeredMessageServices.LargeMessages;

namespace Nimbus.Configuration.LargeMessages
{
    public interface ILargeMessageBodyConfiguration
    {
        ILargeMessageBodyStore LargeMessageBodyStore { get; }
        MaxSmallMessageSizeSetting MaxSmallMessageSize { get; }
        MaxLargeMessageSizeSetting MaxLargeMessageSize { get; }
    }
}