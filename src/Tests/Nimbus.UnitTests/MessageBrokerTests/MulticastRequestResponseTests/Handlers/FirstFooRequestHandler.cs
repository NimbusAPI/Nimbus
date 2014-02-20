using System.Threading.Tasks;
using Nimbus.InfrastructureContracts;
using Nimbus.UnitTests.MessageBrokerTests.MulticastRequestResponseTests.MessageContracts;

namespace Nimbus.UnitTests.MessageBrokerTests.MulticastRequestResponseTests.Handlers
{
    public class FirstFooRequestHandler : IHandleRequest<FooRequest, FooResponse>
    {
        public async Task<FooResponse> Handle(FooRequest request)
        {
            MethodCallCounter.RecordCall<FirstFooRequestHandler>(h => h.Handle(request));
            return new FooResponse(GetType().Name);
        }
    }
}