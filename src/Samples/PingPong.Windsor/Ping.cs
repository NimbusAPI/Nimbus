using Nimbus.MessageContracts;

namespace PingPong.Windsor
{
    public class Ping : BusRequest<Ping, Pong>
    {
        public string Message { get; set; }
    }
}