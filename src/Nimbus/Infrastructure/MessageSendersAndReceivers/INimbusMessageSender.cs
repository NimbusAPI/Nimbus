using System.Threading.Tasks;
using Nimbus.InfrastructureContracts;

namespace Nimbus.Infrastructure.MessageSendersAndReceivers
{
    internal interface INimbusMessageSender
    {
        Task Send(NimbusMessage message);
    }
}