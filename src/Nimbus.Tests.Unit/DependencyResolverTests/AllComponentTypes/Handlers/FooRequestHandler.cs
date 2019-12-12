using System;
using System.Threading.Tasks;
using Nimbus.InfrastructureContracts.Handlers;
using Nimbus.InfrastructureContracts.PropertyInjection;
using Nimbus.Tests.Common.TestUtilities;
using Nimbus.Tests.Unit.DependencyResolverTests.AllComponentTypes.MessageContracts;

#pragma warning disable 4014

namespace Nimbus.Tests.Unit.DependencyResolverTests.AllComponentTypes.Handlers
{
    public class FooRequestHandler : IHandleRequest<FooRequest, FooResponse>, IRequireBusId
    {
        public async Task<FooResponse> Handle(FooRequest request)
        {
            MethodCallCounter.ForInstance(BusId).RecordCall<FooRequestHandler>(h => h.Handle(request));
            return new FooResponse();
        }

        public Guid BusId { get; set; }
    }
}