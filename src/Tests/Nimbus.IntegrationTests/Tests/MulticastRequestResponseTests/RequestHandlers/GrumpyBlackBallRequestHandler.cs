using System.Threading.Tasks;
using Nimbus.InfrastructureContracts;
using Nimbus.IntegrationTests.Tests.MulticastRequestResponseTests.MessageContracts;

namespace Nimbus.IntegrationTests.Tests.MulticastRequestResponseTests.RequestHandlers
{
    public class GrumpyBlackBallRequestHandler : IHandleRequest<BlackBallRequest, BlackBallResponse>
    {
        public async Task<BlackBallResponse> Handle(BlackBallRequest request)
        {
            return new BlackBallResponse
                   {
                       IsBlackBalled = true,
                   };
        }
    }
}