using System.Threading.Tasks;
using Nimbus.InfrastructureContracts;
using Nimbus.IntegrationTests.Tests.SimpleRequestResponseTests.MessageContracts;

namespace Nimbus.IntegrationTests.Tests.SimpleRequestResponseTests.RequestHandlers
{
    public class SomeRequestHandler : IHandleRequest<SomeRequest, SomeResponse>
    {
        public async Task<SomeResponse> Handle(SomeRequest request)
        {
            return new SomeResponse();
        }
    }
}