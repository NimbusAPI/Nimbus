using Nimbus.MessageContracts;

namespace PingPong.PathGenerator
{
    public class Pong : IBusResponse
    {
        public string Message { get; set; }
    }
}