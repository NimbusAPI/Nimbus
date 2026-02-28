using System;
using System.Threading.Tasks;
using Nimbus.InfrastructureContracts.Handlers;
using Nimbus.InfrastructureContracts.PropertyInjection;
using Nimbus.Tests.Common.TestUtilities;
using Nimbus.Tests.Unit.DependencyResolverTests.AllComponentTypes.MessageContracts;

#pragma warning disable 4014

namespace Nimbus.Tests.Unit.DependencyResolverTests.AllComponentTypes.Handlers
{
    public class BrokerTestCommandHandler : IHandleCommand<FooCommand>, IRequireBusId
    {
        public async Task Handle(FooCommand busCommand)
        {
            MethodCallCounter.ForInstance(BusId).RecordCall<BrokerTestCommandHandler>(h => h.Handle(busCommand));
        }

        public Guid BusId { get; set; }
    }
}