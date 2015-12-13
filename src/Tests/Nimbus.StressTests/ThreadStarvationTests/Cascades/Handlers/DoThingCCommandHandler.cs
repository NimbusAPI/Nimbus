using System.Threading.Tasks;
using Nimbus.Handlers;
using Nimbus.StressTests.ThreadStarvationTests.Cascades.MessageContracts;
using Nimbus.Tests.Common;

namespace Nimbus.StressTests.ThreadStarvationTests.Cascades.Handlers
{
#pragma warning disable 1998 // This async method lacks 'await' operators and will run synchronously.
	public class DoThingCCommandHandler : IHandleCommand<DoThingCCommand>
    {
        public Task Handle(DoThingCCommand busCommand)
        {
			  MethodCallCounter.RecordCall<DoThingCCommandHandler>(h => h.Handle(busCommand));
			  return Task.Delay(1);
		  }
    }
}