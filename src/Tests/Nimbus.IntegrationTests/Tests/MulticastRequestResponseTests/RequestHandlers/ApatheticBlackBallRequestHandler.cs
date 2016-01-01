using System.Threading.Tasks;
using Nimbus.Handlers;
using Nimbus.IntegrationTests.Tests.MulticastRequestResponseTests.MessageContracts;
using Nimbus.Tests.Common;
using Nimbus.Tests.Common.TestUtilities;

namespace Nimbus.IntegrationTests.Tests.MulticastRequestResponseTests.RequestHandlers
{
    public class ApatheticBlackBallRequestHandler : IHandleMulticastRequest<BlackBallRequest, BlackBallResponse>
    {
        public Task<BlackBallResponse> Handle(BlackBallRequest request)
        {
            MethodCallCounter.RecordCall<ApatheticBlackBallRequestHandler>(handler => handler.Handle(request));

            return Task.FromResult<BlackBallResponse>(null);
        }
    }
}