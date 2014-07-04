using System.Threading.Tasks;
using Nimbus.Handlers;
using Nimbus.IntegrationTests.Tests.AuditingInterceptorTests.MessageTypes;

namespace Nimbus.IntegrationTests.Tests.AuditingInterceptorTests.Handlers
{
    public class SomeRequestHandler : IHandleRequest<SomeRequest, SomeResponse>
    {
        public Task<SomeResponse> Handle(SomeRequest request)
        {
            return Task.FromResult(new SomeResponse());
        }
    }
}