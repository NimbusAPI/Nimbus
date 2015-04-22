using Nimbus.MessageContracts;

namespace PingPong.Unity
{
    public class Pong : IBusResponse
    {
        public string Message { get; set; }
    }
}