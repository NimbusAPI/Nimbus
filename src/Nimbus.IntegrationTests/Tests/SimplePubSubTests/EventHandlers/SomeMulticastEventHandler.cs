using Nimbus.InfrastructureContracts;
using Nimbus.IntegrationTests.Tests.SimplePubSubTests.MessageContracts;

namespace Nimbus.IntegrationTests.Tests.SimplePubSubTests.EventHandlers
{
    public class SomeMulticastEventHandler : IHandleMulticastEvent<SomeEventWeOnlyHandleViaMulticast>
    {
        public void Handle(SomeEventWeOnlyHandleViaMulticast busEvent)
        {
            MethodCallCounter.RecordCall<SomeMulticastEventHandler>(h => h.Handle(busEvent));
        }
    }
}