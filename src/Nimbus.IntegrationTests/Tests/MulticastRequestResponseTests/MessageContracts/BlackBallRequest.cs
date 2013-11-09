using Nimbus.MessageContracts;

namespace Nimbus.IntegrationTests.Tests.MulticastRequestResponseTests.MessageContracts
{
    public class BlackBallRequest : BusRequest<BlackBallRequest, BlackBallResponse>
    {
        public string ProspectiveMemberName { get; set; }
    }
}