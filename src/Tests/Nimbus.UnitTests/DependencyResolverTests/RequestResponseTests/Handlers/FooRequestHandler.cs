using System.Threading.Tasks;
using Nimbus.Handlers;
using Nimbus.UnitTests.DependencyResolverTests.RequestResponseTests.MessageContracts;

#pragma warning disable 4014

namespace Nimbus.UnitTests.DependencyResolverTests.RequestResponseTests.Handlers
{
    public class FooRequestHandler : IHandleRequest<FooRequest, FooResponse>
    {
        public async Task<FooResponse> Handle(FooRequest request)
        {
            MethodCallCounter.RecordCall<FooRequestHandler>(h => h.Handle(request));
            return new FooResponse();
        }
    }
}