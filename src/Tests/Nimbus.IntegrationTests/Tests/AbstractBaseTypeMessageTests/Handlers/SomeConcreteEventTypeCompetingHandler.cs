using System.Threading.Tasks;
using Nimbus.Handlers;
using Nimbus.IntegrationTests.Tests.AbstractBaseTypeMessageTests.MessageContracts;
using Nimbus.Tests.Common;

namespace Nimbus.IntegrationTests.Tests.AbstractBaseTypeMessageTests.Handlers
{
    public class SomeConcreteEventTypeCompetingHandler : IHandleCompetingEvent<SomeConcreteEventType>
    {
        public async Task Handle(SomeConcreteEventType busEvent)
        {
            MethodCallCounter.RecordCall<SomeConcreteEventTypeCompetingHandler>(ch => ch.Handle(busEvent));
        }
    }
}