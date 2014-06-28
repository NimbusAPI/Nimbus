using System.Threading.Tasks;
using Nimbus.Handlers;
using Nimbus.IntegrationTests.Tests.AbstractBaseTypeMessageTests.MessageContracts;

namespace Nimbus.IntegrationTests.Tests.AbstractBaseTypeMessageTests.Handlers
{
    public class SomeConcreteEventTypeMulticastHandler : IHandleMulticastEvent<SomeConcreteEventType>
    {
        public async Task Handle(SomeConcreteEventType busEvent)
        {
            MethodCallCounter.RecordCall<SomeConcreteEventTypeMulticastHandler>(ch => ch.Handle(busEvent));
        }
    }
}