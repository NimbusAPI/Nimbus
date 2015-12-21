using System.Threading.Tasks;
using Nimbus.Handlers;
using Nimbus.IntegrationTests.Tests.MulticastRequestResponseTests.MessageContracts;
using Nimbus.Tests.Common;
using Nimbus.Tests.Common.TestUtilities;

namespace Nimbus.IntegrationTests.Tests.MulticastRequestResponseTests.RequestHandlers
{
    public class GrumpyBlackBallRequestHandler : IHandleMulticastRequest<BlackBallRequest, BlackBallResponse>
    {
        public async Task<BlackBallResponse> Handle(BlackBallRequest request)
        {
            MethodCallCounter.RecordCall<GrumpyBlackBallRequestHandler>(handler => handler.Handle(request));

            return new BlackBallResponse
                   {
                       IsBlackBalled = true,
                   };
        }
    }
}