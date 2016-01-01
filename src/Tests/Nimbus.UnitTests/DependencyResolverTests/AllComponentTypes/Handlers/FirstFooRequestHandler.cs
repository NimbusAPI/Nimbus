using System.Threading.Tasks;
using Nimbus.Handlers;
using Nimbus.Tests.Common;
using Nimbus.Tests.Common.TestUtilities;
using Nimbus.UnitTests.DependencyResolverTests.AllComponentTypes.MessageContracts;

#pragma warning disable 4014

namespace Nimbus.UnitTests.DependencyResolverTests.AllComponentTypes.Handlers
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