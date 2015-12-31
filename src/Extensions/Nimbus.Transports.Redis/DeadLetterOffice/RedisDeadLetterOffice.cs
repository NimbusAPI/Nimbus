using System;
using System.Threading.Tasks;

namespace Nimbus.Transports.Redis.DeadLetterOffice
{
    internal class RedisDeadLetterOffice:IDeadLetterOffice
    {
        public Task<NimbusMessage> Peek()
        {
            throw new NotImplementedException();
        }

        public Task<NimbusMessage> Pop()
        {
            throw new NotImplementedException();
        }

        public Task Post(NimbusMessage message)
        {
            throw new NotImplementedException();
        }

        public Task<int> Count()
        {
            throw new NotImplementedException();
        }
    }
}