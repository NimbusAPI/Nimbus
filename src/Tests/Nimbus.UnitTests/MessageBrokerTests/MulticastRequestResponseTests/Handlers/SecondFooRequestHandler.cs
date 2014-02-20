using Nimbus.InfrastructureContracts;
using Nimbus.UnitTests.MessageBrokerTests.MulticastRequestResponseTests.MessageContracts;

namespace Nimbus.UnitTests.MessageBrokerTests.MulticastRequestResponseTests.Handlers
{
    public class SecondFooRequestHandler : IHandleRequest<FooRequest, FooResponse>
    {
        public FooResponse Handle(FooRequest request)
        {
            MethodCallCounter.RecordCall<SecondFooRequestHandler>(h => h.Handle(request));
            return new FooResponse(GetType().Name);
        }
    }
}