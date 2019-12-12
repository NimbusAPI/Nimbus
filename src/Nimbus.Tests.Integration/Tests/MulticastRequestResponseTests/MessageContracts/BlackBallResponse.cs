using Nimbus.MessageContracts;

namespace Nimbus.Tests.Integration.Tests.MulticastRequestResponseTests.MessageContracts
{
    public class BlackBallResponse : IBusMulticastResponse
    {
        public bool IsBlackBalled { get; set; }
    }
}