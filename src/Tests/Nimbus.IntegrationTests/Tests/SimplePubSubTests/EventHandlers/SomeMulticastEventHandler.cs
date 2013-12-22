using Nimbus.InfrastructureContracts;
using Nimbus.IntegrationTests.Tests.SimplePubSubTests.MessageContracts;

namespace Nimbus.IntegrationTests.Tests.SimplePubSubTests.EventHandlers
{
    public class SomeMulticastEventHandler : IHandleMulticastEvent<SomeEventWeOnlyHandleViaMulticast>, IHandleMulticastEvent<SomeEventWeHandleViaMulticastAndCompetition>
    {
        public void Handle(SomeEventWeOnlyHandleViaMulticast busEvent)
        {
            MethodCallCounter.RecordCall<SomeMulticastEventHandler>(h => h.Handle(busEvent));
        }

        public void Handle(SomeEventWeHandleViaMulticastAndCompetition busEvent)
        {
            MethodCallCounter.RecordCall<SomeMulticastEventHandler>(h => h.Handle(busEvent));
        }
    }
}