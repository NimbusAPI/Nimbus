using System;
using Nimbus.InfrastructureContracts;
using Nimbus.IntegrationTests.Tests.SimpleRequestResponseTests.MessageContracts;

namespace Nimbus.IntegrationTests.Tests.SimpleRequestResponseTests.RequestHandlers
{
    public class SomeRequestHandler : IHandleRequest<SomeRequest, SomeResponse>
    {
        public SomeResponse Handle(SomeRequest request)
        {
            throw new NotImplementedException();
        }
    }
}