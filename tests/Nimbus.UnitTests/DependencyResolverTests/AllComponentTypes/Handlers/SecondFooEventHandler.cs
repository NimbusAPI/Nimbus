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
    public class SecondFooEventHandler : IHandleCompetingEvent<FooEvent>, IRequireBusId
    {
        public async Task Handle(FooEvent busEvent)
        {
            MethodCallCounter.ForInstance(BusId).RecordCall<SecondFooEventHandler>(h => h.Handle(busEvent));
        }

        public Guid BusId { get; set; }
    }
}