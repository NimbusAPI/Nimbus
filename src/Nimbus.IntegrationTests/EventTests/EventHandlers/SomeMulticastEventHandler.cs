using System;
using Nimbus.InfrastructureContracts;
using Nimbus.IntegrationTests.EventTests.MessageContracts;

namespace Nimbus.IntegrationTests.EventTests.EventHandlers
{
    public class SomeMulticastEventHandler : IHandleMulticastEvent<SomeEventWeOnlyHandleViaMulticast>
    {
        public void Handle(SomeEventWeOnlyHandleViaMulticast busEvent)
        {
            throw new NotImplementedException();
        }
    }
}