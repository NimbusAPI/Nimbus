using System.Threading.Tasks;
using Nimbus.Handlers;
using Nimbus.IntegrationTests.Tests.AbstractBaseTypeMessageTests.MessageContracts;
using Nimbus.Tests.Common;
using Nimbus.Tests.Common.TestUtilities;

namespace Nimbus.IntegrationTests.Tests.AbstractBaseTypeMessageTests.Handlers
{
    public class SomeConcreteCommandTypeHandler : IHandleCommand<SomeConcreteCommandType>
    {
        public async Task Handle(SomeConcreteCommandType busCommand)
        {
            MethodCallCounter.RecordCall<SomeConcreteCommandTypeHandler>(ch => ch.Handle(busCommand));
        }
    }
}