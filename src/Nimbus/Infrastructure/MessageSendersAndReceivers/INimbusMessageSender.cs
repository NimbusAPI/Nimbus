using System.Threading.Tasks;

namespace Nimbus.Infrastructure.MessageSendersAndReceivers
{
    internal interface INimbusMessageSender
    {
        Task Send(NimbusMessage message);
    }
}