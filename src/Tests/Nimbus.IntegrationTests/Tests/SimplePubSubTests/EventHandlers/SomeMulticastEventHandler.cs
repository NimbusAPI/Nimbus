using System.Threading.Tasks;
using Nimbus.InfrastructureContracts;
using Nimbus.IntegrationTests.Tests.SimplePubSubTests.MessageContracts;

namespace Nimbus.IntegrationTests.Tests.SimplePubSubTests.EventHandlers
{
    public class SomeMulticastEventHandler : IHandleMulticastEvent<SomeEventWeOnlyHandleViaMulticast>, IHandleMulticastEvent<SomeEventWeHandleViaMulticastAndCompetition>
    {
        public async Task Handle(SomeEventWeOnlyHandleViaMulticast busEvent)
        {
            MethodCallCounter.RecordCall<SomeMulticastEventHandler>(h => h.Handle(busEvent));
        }

        public async Task Handle(SomeEventWeHandleViaMulticastAndCompetition busEvent)
        {
            MethodCallCounter.RecordCall<SomeMulticastEventHandler>(h => h.Handle(busEvent));
        }
    }
}