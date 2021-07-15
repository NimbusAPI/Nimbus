using System.Threading.Tasks;
using Amqp;
using Nimbus.InfrastructureContracts;

namespace Nimbus.Transports.Amqp.Messages
{
    public interface IMessageFactory
    {
        Task<Message> BuildMessage(NimbusMessage nimbusMessage);
        Task<NimbusMessage> BuildNimbusMessage(Message message);
    }
}