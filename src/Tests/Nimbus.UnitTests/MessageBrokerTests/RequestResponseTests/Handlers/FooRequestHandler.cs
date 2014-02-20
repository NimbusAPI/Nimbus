using Nimbus.InfrastructureContracts;
using Nimbus.UnitTests.MessageBrokerTests.RequestResponseTests.MessageContracts;

namespace Nimbus.UnitTests.MessageBrokerTests.RequestResponseTests.Handlers
{
    public class FooRequestHandler : IHandleRequest<FooRequest, FooResponse>
    {
        public FooResponse Handle(FooRequest request)
        {
            MethodCallCounter.RecordCall<FooRequestHandler>(h => h.Handle(request));
            return new FooResponse();
        }
    }
}