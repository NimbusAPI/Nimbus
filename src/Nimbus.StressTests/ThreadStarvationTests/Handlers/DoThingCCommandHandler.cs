using System.Threading.Tasks;
using Nimbus.Handlers;
using Nimbus.StressTests.ThreadStarvationTests.MessageContracts;
using Nimbus.Tests.Common;

namespace Nimbus.StressTests.ThreadStarvationTests.Handlers
{
    public class DoThingCCommandHandler : IHandleCommand<DoThingCCommand>
    {
        public async Task Handle(DoThingCCommand busCommand)
        {
            MethodCallCounter.RecordCall<DoThingCCommandHandler>(h => h.Handle(busCommand));
        }
    }
}