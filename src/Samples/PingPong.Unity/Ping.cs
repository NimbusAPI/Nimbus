using Nimbus.MessageContracts;

namespace PingPong.Unity
{
    public class Ping : BusRequest<Ping, Pong>
    {
        public string Message { get; set; }
    }
}