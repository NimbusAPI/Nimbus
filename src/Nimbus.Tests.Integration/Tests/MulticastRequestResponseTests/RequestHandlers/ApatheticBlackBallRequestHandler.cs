using System;
using System.Threading.Tasks;
using Nimbus.InfrastructureContracts.Handlers;
using Nimbus.InfrastructureContracts.PropertyInjection;
using Nimbus.Tests.Common.TestUtilities;
using Nimbus.Tests.Integration.Tests.MulticastRequestResponseTests.MessageContracts;

namespace Nimbus.Tests.Integration.Tests.MulticastRequestResponseTests.RequestHandlers
{
    public class ApatheticBlackBallRequestHandler : IHandleMulticastRequest<BlackBallRequest, BlackBallResponse>, IRequireBusId
    {
        public Task<BlackBallResponse> Handle(BlackBallRequest request)
        {
            MethodCallCounter.ForInstance(BusId).RecordCall<ApatheticBlackBallRequestHandler>(handler => handler.Handle(request));

            return Task.FromResult<BlackBallResponse>(null);
        }

        public Guid BusId { get; set; }
    }
}