using System;
using Nimbus.InfrastructureContracts;
using Nimbus.IntegrationTests.Tests.SimplePubSubTests.MessageContracts;

namespace Nimbus.IntegrationTests.Tests.SimplePubSubTests.EventHandlers
{
    public class SomeMulticastEventHandler : IHandleMulticastEvent<SomeEventWeOnlyHandleViaMulticast>
    {
        public void Handle(SomeEventWeOnlyHandleViaMulticast busEvent)
        {
            throw new NotImplementedException();
        }
    }
}