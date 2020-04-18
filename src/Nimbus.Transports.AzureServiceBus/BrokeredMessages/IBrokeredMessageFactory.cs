using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Nimbus.InfrastructureContracts;

namespace Nimbus.Transports.AzureServiceBus.Messages
{
    internal interface IBrokeredMessageFactory
    {
        Task<Message> BuildMessage(NimbusMessage message);
        Task<NimbusMessage> BuildNimbusMessage(Message message);
    }
}