using Nimbus.MessageContracts;

namespace Nimbus.IntegrationTests.Tests.MulticastRequestResponseTests.MessageContracts
{
    public class BlackBallResponse : IBusResponse
    {
        public bool IsBlackBalled { get; set; }
    }
}