using System;
using Nimbus.InfrastructureContracts;
using Nimbus.IntegrationTests.MessageContracts;

namespace Nimbus.IntegrationTests.Handlers
{
    public class SomeRequestHandler : IHandleRequest<SomeRequest, SomeResponse>
    {
        public SomeResponse Handle(SomeRequest request)
        {
            throw new NotImplementedException();
        }
    }
}