using System;
using Nimbus.InfrastructureContracts;
using Nimbus.IntegrationTests.Tests.SimplePubSubTests.MessageContracts;

namespace Nimbus.IntegrationTests.Tests.SimplePubSubTests.EventHandlers
{
    public class SomeCompetingEventHandler : IHandleCompetingEvent<SomeEventWeOnlyHandleViaCompetition>
    {
        public void Handle(SomeEventWeOnlyHandleViaCompetition busEvent)
        {
            throw new NotImplementedException();
        }
    }
}