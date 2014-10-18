using System.Threading.Tasks;
using Nimbus.Handlers;
using Nimbus.StressTests.ThreadStarvationTests.Cascades.MessageContracts;
using Nimbus.Tests.Common;

namespace Nimbus.StressTests.ThreadStarvationTests.Cascades.Handlers
{
    public class DoThingCCommandHandler : IHandleCommand<DoThingCCommand>
    {
        public async Task Handle(DoThingCCommand busCommand)
        {
            MethodCallCounter.RecordCall<DoThingCCommandHandler>(h => h.Handle(busCommand));
        }
    }
}