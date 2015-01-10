using Nimbus.MessageContracts;

namespace PingPong.PathGenerator
{
    public class Ping : BusRequest<Ping, Pong>
    {
        public string Message { get; set; }
    }
}