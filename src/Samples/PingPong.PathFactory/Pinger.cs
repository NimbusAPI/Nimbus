using System;
using System.Threading.Tasks;
using Nimbus;

namespace PingPong.PathGenerator
{
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
            var response = await _bus.Request(new Ping {Message = (message.ToLowerInvariant() == "ping" ? "Pong" : message)});
            return response.Message;
        }
    }
}