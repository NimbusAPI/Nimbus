using System;
using System.Threading.Tasks;
using Nimbus.Handlers;
using Nimbus.IntegrationTests.Tests.AbstractBaseTypeMessageTests.MessageContracts;
using Nimbus.PropertyInjection;
using Nimbus.Tests.Common;
using Nimbus.Tests.Common.TestUtilities;

namespace Nimbus.IntegrationTests.Tests.AbstractBaseTypeMessageTests.Handlers
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