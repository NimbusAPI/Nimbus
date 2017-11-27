using System;
using System.Threading.Tasks;
using Nimbus.Handlers;
using Nimbus.PropertyInjection;
using Nimbus.Tests.Common;
using Nimbus.Tests.Common.TestUtilities;
using Nimbus.UnitTests.DependencyResolverTests.AllComponentTypes.MessageContracts;

#pragma warning disable 4014

namespace Nimbus.UnitTests.DependencyResolverTests.AllComponentTypes.Handlers
{
    public class SecondFooRequestHandler : IHandleRequest<FooRequest, FooResponse>, IRequireBusId
    {
        public async Task<FooResponse> Handle(FooRequest request)
        {
            MethodCallCounter.ForInstance(BusId).RecordCall<SecondFooRequestHandler>(h => h.Handle(request));
            return new FooResponse(GetType().Name);
        }

        public Guid BusId { get; set; }
    }
}