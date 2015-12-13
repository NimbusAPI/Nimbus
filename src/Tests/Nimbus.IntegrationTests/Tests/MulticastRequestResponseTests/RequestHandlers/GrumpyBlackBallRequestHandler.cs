using System.Threading.Tasks;
using Nimbus.Handlers;
using Nimbus.IntegrationTests.Tests.MulticastRequestResponseTests.MessageContracts;
using Nimbus.Tests.Common;

namespace Nimbus.IntegrationTests.Tests.MulticastRequestResponseTests.RequestHandlers
{
#pragma warning disable 4014 // Because this call is not awaited, execution of the current method continues before the call is completed.
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