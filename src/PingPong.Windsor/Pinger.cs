using System;
using System.Collections;
using System.Collections.Generic;
using Nimbus;
using Nimbus.InfrastructureContracts;
using System.Threading.Tasks;

namespace PingPong.Windsor
{
    public interface IPinger
    {
        Task<string> Ping(string message);
    }

    public class Pinger : IPinger
    {
        private readonly IBus _bus;

        public Pinger(IBus bus)
        {
            _bus = bus;
        }

        public async Task<string> Ping(string message)
        {
            Console.WriteLine("Sending message: '{0}'", message);
            var response = await _bus.Request<Ping, Pong>(new Ping { Message = (message.ToLowerInvariant() == "ping" ? "Pong" : message) });
            return response.Message;
        }
    }
}