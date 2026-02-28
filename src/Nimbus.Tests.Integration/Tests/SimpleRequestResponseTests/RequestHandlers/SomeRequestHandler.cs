using System.Threading.Tasks;
using Nimbus.InfrastructureContracts.Handlers;
using Nimbus.Tests.Integration.Tests.SimpleRequestResponseTests.MessageContracts;

namespace Nimbus.Tests.Integration.Tests.SimpleRequestResponseTests.RequestHandlers
{
    public class SomeRequestHandler : IHandleRequest<SomeRequest, SomeResponse>
    {
        public async Task<SomeResponse> Handle(SomeRequest request)
        {
            return new SomeResponse();
        }
    }
}