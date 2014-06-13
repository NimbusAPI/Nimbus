using Nimbus.MessageContracts;

namespace Nimbus.IntegrationTests.Tests.MulticastRequestResponseTests.MessageContracts
{
    public class BlackBallResponse : IBusMulticastResponse
    {
        public bool IsBlackBalled { get; set; }
    }
}