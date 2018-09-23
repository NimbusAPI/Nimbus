using System;
using System.Threading;
using System.Threading.Tasks;
using Nimbus.Handlers;
using Nimbus.IntegrationTests.Tests.MulticastRequestResponseTests.MessageContracts;
using Nimbus.PropertyInjection;
using Nimbus.Tests.Common.TestUtilities;

namespace Nimbus.IntegrationTests.Tests.MulticastRequestResponseTests.RequestHandlers
{
    public class SlowBlackBallRequestHandler : IHandleMulticastRequest<BlackBallRequest, BlackBallResponse>, IRequireBusId
    {
        public static SemaphoreSlim HandlerThrottle { get; private set; }

        public static void Reset()
        {
            HandlerThrottle = new SemaphoreSlim(0, int.MaxValue);
        }

        static SlowBlackBallRequestHandler()
        {
            Reset();
        }

        public async Task<BlackBallResponse> Handle(BlackBallRequest request)
        {
            MethodCallCounter.ForInstance(BusId).RecordCall<SlowBlackBallRequestHandler>(handler => handler.Handle(request));

            await HandlerThrottle.WaitAsync();

            return new BlackBallResponse
                   {
                       IsBlackBalled = false
                   };
        }

        public Guid BusId { get; set; }
    }
}