using System.Threading.Tasks;
using Nimbus.Handlers;
using Nimbus.UnitTests.DependencyResolverTests.MulticastRequestResponseTests.MessageContracts;

#pragma warning disable 4014

namespace Nimbus.UnitTests.DependencyResolverTests.MulticastRequestResponseTests.Handlers
{
    public class SecondFooRequestHandler : IHandleRequest<FooRequest, FooResponse>
    {
        public async Task<FooResponse> Handle(FooRequest request)
        {
            MethodCallCounter.RecordCall<SecondFooRequestHandler>(h => h.Handle(request));
            return new FooResponse(GetType().Name);
        }
    }
}