using System;
using System.Threading.Tasks;
using Nimbus.InfrastructureContracts.Handlers;
using Nimbus.InfrastructureContracts.PropertyInjection;
using Nimbus.Tests.Common.TestUtilities;
using Nimbus.Tests.Integration.Tests.AbstractBaseTypeMessageTests.MessageContracts;

namespace Nimbus.Tests.Integration.Tests.AbstractBaseTypeMessageTests.Handlers
{
    public class SomeConcreteCommandTypeHandler : IHandleCommand<SomeConcreteCommandType>, IRequireBusId
    {
        public async Task Handle(SomeConcreteCommandType busCommand)
        {
            MethodCallCounter.ForInstance(BusId).RecordCall<SomeConcreteCommandTypeHandler>(ch => ch.Handle(busCommand));
        }

        public Guid BusId { get; set; }
    }
}