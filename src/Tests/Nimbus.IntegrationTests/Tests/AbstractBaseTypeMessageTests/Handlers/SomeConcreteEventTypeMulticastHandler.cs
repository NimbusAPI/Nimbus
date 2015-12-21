using System.Threading.Tasks;
using Nimbus.Handlers;
using Nimbus.IntegrationTests.Tests.AbstractBaseTypeMessageTests.MessageContracts;
using Nimbus.Tests.Common;
using Nimbus.Tests.Common.TestUtilities;

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