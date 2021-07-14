using System.Threading.Tasks;
using Nimbus.InfrastructureContracts;

namespace Nimbus.Transports.Amqp.DeadLetterOffice
{
    public class AmqpDeadLetterOffice : IDeadLetterOffice
    {
        public Task<NimbusMessage> Peek()
        {
            throw new System.NotImplementedException();
        }

        public Task<NimbusMessage> Pop()
        {
            throw new System.NotImplementedException();
        }

        public Task Post(NimbusMessage message)
        {
            throw new System.NotImplementedException();
        }

        public Task<int> Count()
        {
            throw new System.NotImplementedException();
        }
    }
}