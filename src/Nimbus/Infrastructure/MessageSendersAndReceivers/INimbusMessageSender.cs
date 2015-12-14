using System.Threading.Tasks;

namespace Nimbus.Infrastructure.MessageSendersAndReceivers
{
    public interface INimbusMessageSender
    {
        Task Send(NimbusMessage message);
    }
}