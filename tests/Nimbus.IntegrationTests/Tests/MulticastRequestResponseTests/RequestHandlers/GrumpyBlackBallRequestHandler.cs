using System;
using System.Threading.Tasks;
using Nimbus.Handlers;
using Nimbus.IntegrationTests.Tests.MulticastRequestResponseTests.MessageContracts;
using Nimbus.PropertyInjection;
using Nimbus.Tests.Common;
using Nimbus.Tests.Common.TestUtilities;

namespace Nimbus.IntegrationTests.Tests.MulticastRequestResponseTests.RequestHandlers
{
    public class GrumpyBlackBallRequestHandler : IHandleMulticastRequest<BlackBallRequest, BlackBallResponse>, IRequireBusId
    {
        public async Task<BlackBallResponse> Handle(BlackBallRequest request)
        {
            MethodCallCounter.ForInstance(BusId).RecordCall<GrumpyBlackBallRequestHandler>(handler => handler.Handle(request));

            return new BlackBallResponse
                   {
                       IsBlackBalled = true,
                   };
        }

        public Guid BusId { get; set; }
    }
}