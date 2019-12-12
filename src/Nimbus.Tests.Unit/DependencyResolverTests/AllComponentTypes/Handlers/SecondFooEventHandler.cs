using System;
using System.Threading.Tasks;
using Nimbus.InfrastructureContracts.Handlers;
using Nimbus.InfrastructureContracts.PropertyInjection;
using Nimbus.Tests.Common.TestUtilities;
using Nimbus.Tests.Unit.DependencyResolverTests.AllComponentTypes.MessageContracts;

#pragma warning disable 4014

namespace Nimbus.Tests.Unit.DependencyResolverTests.AllComponentTypes.Handlers
{
    public class SecondFooEventHandler : IHandleCompetingEvent<FooEvent>, IRequireBusId
    {
        public async Task Handle(FooEvent busEvent)
        {
            MethodCallCounter.ForInstance(BusId).RecordCall<SecondFooEventHandler>(h => h.Handle(busEvent));
        }

        public Guid BusId { get; set; }
    }
}