using System;
using Nimbus.InfrastructureContracts;
using Nimbus.IntegrationTests.EventTests.MessageContracts;

namespace Nimbus.IntegrationTests.EventTests.EventHandlers
{
    public class SomeCompetingEventHandler : IHandleCompetingEvent<SomeEventWeOnlyHandleViaCompetition>
    {
        public void Handle(SomeEventWeOnlyHandleViaCompetition busEvent)
        {
            throw new NotImplementedException();
        }
    }
}