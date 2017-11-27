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
    public class BrokerTestCommandHandler : IHandleCommand<FooCommand>, IRequireBusId
    {
        public async Task Handle(FooCommand busCommand)
        {
            MethodCallCounter.ForInstance(BusId).RecordCall<BrokerTestCommandHandler>(h => h.Handle(busCommand));
        }

        public Guid BusId { get; set; }
    }
}