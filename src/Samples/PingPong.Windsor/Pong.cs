using Nimbus.MessageContracts;

namespace PingPong.Windsor
{
    public class Pong : IBusResponse
    {
        public string Message { get; set; }
    }
}