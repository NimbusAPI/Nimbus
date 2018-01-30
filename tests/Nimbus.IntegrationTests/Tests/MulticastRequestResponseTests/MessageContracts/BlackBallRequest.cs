using Nimbus.MessageContracts;

namespace Nimbus.IntegrationTests.Tests.MulticastRequestResponseTests.MessageContracts
{
    public class BlackBallRequest : IBusMulticastRequest<BlackBallRequest, BlackBallResponse>
    {
        public string ProspectiveMemberName { get; set; }
    }
}