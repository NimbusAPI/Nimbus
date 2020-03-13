using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Nimbus.InfrastructureContracts;

namespace Nimbus.Transports.AzureServiceBus.Messages
{
    internal interface IMessageFactory
    {
        Task<Message> BuildMessage(NimbusMessage message);
        Task<NimbusMessage> BuildNimbusMessage(Message message);
    }
}