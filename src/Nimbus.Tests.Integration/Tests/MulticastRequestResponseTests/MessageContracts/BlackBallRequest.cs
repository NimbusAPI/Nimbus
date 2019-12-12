using Nimbus.MessageContracts;

namespace Nimbus.Tests.Integration.Tests.MulticastRequestResponseTests.MessageContracts
{
    public class BlackBallRequest : IBusMulticastRequest<BlackBallRequest, BlackBallResponse>
    {
        public string ProspectiveMemberName { get; set; }
    }
}