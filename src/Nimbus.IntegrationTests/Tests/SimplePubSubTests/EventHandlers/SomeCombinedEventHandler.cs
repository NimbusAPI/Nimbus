using Nimbus.InfrastructureContracts;
using Nimbus.IntegrationTests.Tests.SimplePubSubTests.MessageContracts;

namespace Nimbus.IntegrationTests.Tests.SimplePubSubTests.EventHandlers
{
    public class SomeCombinedEventHandler : IHandleMulticastEvent<SomeEventWeHandleViaMulticastAndCompetition>, IHandleCompetingEvent<SomeEventWeHandleViaMulticastAndCompetition>
    {
        void IHandleMulticastEvent<SomeEventWeHandleViaMulticastAndCompetition>.Handle(SomeEventWeHandleViaMulticastAndCompetition busEvent)
        {
        }

        void IHandleCompetingEvent<SomeEventWeHandleViaMulticastAndCompetition>.Handle(SomeEventWeHandleViaMulticastAndCompetition busEvent)
        {
        }
    }
}