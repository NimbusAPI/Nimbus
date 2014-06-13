using System.Threading.Tasks;
using Nimbus.Handlers;
using Nimbus.IntegrationTests.Tests.MulticastRequestResponseTests.MessageContracts;

namespace Nimbus.IntegrationTests.Tests.MulticastRequestResponseTests.RequestHandlers
{
    public class HappyBlackBallRequestHandler : IHandleMulticastRequest<BlackBallRequest, BlackBallResponse>
    {
        public async Task<BlackBallResponse> Handle(BlackBallRequest request)
        {
            return new BlackBallResponse
                   {
                       IsBlackBalled = false,
                   };
        }
    }
}