using System.Threading;
using System.Threading.Tasks;
using Nimbus.InfrastructureContracts.Handlers;
using Nimbus.Tests.Integration.Tests.RequestResponseCorrelationTests.MessageContracts;

namespace Nimbus.Tests.Integration.Tests.RequestResponseCorrelationTests.RequestHandlers
{
    public class SlowRequestHandler : IHandleRequest<SlowRequest, SlowResponse>
    {
        // Holds every SlowRequest open until the test releases it, so the requests are guaranteed to be
        // in flight when the correlator's periodic sweep runs.
        public static TaskCompletionSource<bool> Gate = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        public static int Received;

        public async Task<SlowResponse> Handle(SlowRequest request)
        {
            Interlocked.Increment(ref Received);
            await Gate.Task;
            return new SlowResponse();
        }
    }
}
