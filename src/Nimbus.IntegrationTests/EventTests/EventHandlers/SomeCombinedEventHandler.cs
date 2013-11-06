using System;
using Nimbus.InfrastructureContracts;
using Nimbus.IntegrationTests.EventTests.MessageContracts;

namespace Nimbus.IntegrationTests.EventTests.EventHandlers
{
    public class SomeCombinedEventHandler : IHandleMulticastEvent<SomeEventWeHandleViaMulticastAndCompetition>, IHandleCompetingEvent<SomeEventWeHandleViaMulticastAndCompetition>
    {
        void IHandleMulticastEvent<SomeEventWeHandleViaMulticastAndCompetition>.Handle(SomeEventWeHandleViaMulticastAndCompetition busEvent)
        {
            throw new NotImplementedException();
        }

        void IHandleCompetingEvent<SomeEventWeHandleViaMulticastAndCompetition>.Handle(SomeEventWeHandleViaMulticastAndCompetition busEvent)
        {
            throw new NotImplementedException();
        }
    }
}