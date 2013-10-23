using System;
using Nimbus.InfrastructureContracts;
using Nimbus.IntegrationTests.MessageContracts;

namespace Nimbus.IntegrationTests.Handlers
{
    public class SomeEventHandler : IHandleEvent<SomeEvent>
    {
        public void Handle(SomeEvent busEvent)
        {
            throw new NotImplementedException();
        }
    }
}