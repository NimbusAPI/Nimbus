using System.Threading.Tasks;
using Nimbus.Handlers;
using Nimbus.IntegrationTests.Tests.AbstractBaseTypeMessageTests.MessageContracts;

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