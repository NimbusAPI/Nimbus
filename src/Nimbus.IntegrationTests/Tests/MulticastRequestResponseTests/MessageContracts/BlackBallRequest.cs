using Nimbus.MessageContracts;

namespace Nimbus.IntegrationTests.Tests.MulticastRequestResponseTests.MessageContracts
{
    public class BlackBallRequest : IBusRequest<BlackBallRequest, BlackBallResponse>
    {
        public string ProspectiveMemberName { get; set; }
    }
}