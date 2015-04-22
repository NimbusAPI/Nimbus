﻿using System.Threading.Tasks;
using Nimbus.Handlers;

namespace PingPong.Unity
{
    public class Ponger : IHandleRequest<Ping, Pong>
    {
        public async Task<Pong> Handle(Ping request)
        {
            return new Pong {Message = request.Message};
        }
    }
}