using System.Threading.Tasks;
using Nimbus.Handlers;
using Nimbus.Tests.Common;
using Nimbus.UnitTests.DependencyResolverTests.RequestResponseHandlerResolutionTests.MessageContracts;

#pragma warning disable 4014

namespace Nimbus.UnitTests.DependencyResolverTests.RequestResponseHandlerResolutionTests.Handlers
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