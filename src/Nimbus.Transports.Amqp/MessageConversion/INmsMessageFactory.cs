using System.Threading.Tasks;
using Apache.NMS;
using Nimbus.InfrastructureContracts;

namespace Nimbus.Transports.AMQP.MessageConversion
{
    internal interface INmsMessageFactory
    {
        Task<IBytesMessage> CreateNmsMessage(NimbusMessage nimbusMessage, ISession session);
        Task<NimbusMessage> CreateNimbusMessage(IMessage nmsMessage);
    }
}
