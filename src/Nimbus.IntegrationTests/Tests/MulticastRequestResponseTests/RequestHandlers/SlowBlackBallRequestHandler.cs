using System;
using System.Threading;
using Nimbus.InfrastructureContracts;
using Nimbus.IntegrationTests.Tests.MulticastRequestResponseTests.MessageContracts;

namespace Nimbus.IntegrationTests.Tests.MulticastRequestResponseTests.RequestHandlers
{
    public class SlowBlackBallRequestHandler : IHandleRequest<BlackBallRequest, BlackBallResponse>
    {
        public BlackBallResponse Handle(BlackBallRequest request)
        {
            Thread.Sleep(TimeSpan.FromSeconds(5));

            return new BlackBallResponse
            {
                IsBlackBalled = false,
            };
        }
    }
}