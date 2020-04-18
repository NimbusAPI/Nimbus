using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Nimbus.InfrastructureContracts;

namespace Nimbus.Transports.AzureServiceBus.BrokeredMessages
{
    internal interface IBrokeredMessageFactory
    {
        Task<Message> BuildMessage(NimbusMessage nimbusMessage);
        Task<NimbusMessage> BuildNimbusMessage(Message message);
    }
}