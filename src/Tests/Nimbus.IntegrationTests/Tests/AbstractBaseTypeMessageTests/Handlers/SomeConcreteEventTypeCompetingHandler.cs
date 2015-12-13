using System.Threading.Tasks;
using Nimbus.Handlers;
using Nimbus.IntegrationTests.Tests.AbstractBaseTypeMessageTests.MessageContracts;
using Nimbus.Tests.Common;

namespace Nimbus.IntegrationTests.Tests.AbstractBaseTypeMessageTests.Handlers
{
#pragma warning disable 4014 // Because this call is not awaited, execution of the current method continues before the call is completed.
	public class SomeConcreteEventTypeCompetingHandler : IHandleCompetingEvent<SomeConcreteEventType>
    {
        public async Task Handle(SomeConcreteEventType busEvent)
        {
            MethodCallCounter.RecordCall<SomeConcreteEventTypeCompetingHandler>(ch => ch.Handle(busEvent));
        }
    }
}