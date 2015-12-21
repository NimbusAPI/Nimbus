using System;
using System.Threading.Tasks;
using Nimbus.Handlers;
using Nimbus.IntegrationTests.Tests.MulticastRequestResponseTests.MessageContracts;
using Nimbus.Tests.Common;
using Nimbus.Tests.Common.TestUtilities;

namespace Nimbus.IntegrationTests.Tests.MulticastRequestResponseTests.RequestHandlers
{
    public class SlowBlackBallRequestHandler : IHandleMulticastRequest<BlackBallRequest, BlackBallResponse>
    {
        public async Task<BlackBallResponse> Handle(BlackBallRequest request)
        {
            MethodCallCounter.RecordCall<SlowBlackBallRequestHandler>(handler => handler.Handle(request));

            await Task.Delay(TimeSpan.FromSeconds(3));

            return new BlackBallResponse
                   {
                       IsBlackBalled = false,
                   };
        }
    }
}