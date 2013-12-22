using Nimbus.InfrastructureContracts;
using Nimbus.IntegrationTests.Tests.MulticastRequestResponseTests.MessageContracts;

namespace Nimbus.IntegrationTests.Tests.MulticastRequestResponseTests.RequestHandlers
{
    public class HappyBlackBallRequestHandler : IHandleRequest<BlackBallRequest, BlackBallResponse>
    {
        public BlackBallResponse Handle(BlackBallRequest request)
        {
            return new BlackBallResponse
                   {
                       IsBlackBalled = false,
                   };
        }
    }
}