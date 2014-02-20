using Nimbus.InfrastructureContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PingPong.Windsor
{
    public class Ponger : IHandleRequest<Ping, Pong>
    {
        public async Task<Pong> Handle(Ping request)
        {
            return new Pong { Message = request.Message };
        }
    }
}
