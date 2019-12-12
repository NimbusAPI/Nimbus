using System.Threading.Tasks;
using Nimbus.InfrastructureContracts.Handlers;
using Nimbus.Tests.Integration.Tests.AuditingInterceptorTests.MessageTypes;

namespace Nimbus.Tests.Integration.Tests.AuditingInterceptorTests.Handlers
{
    public class SomeRequestHandler : IHandleRequest<SomeRequest, SomeResponse>
    {
        public Task<SomeResponse> Handle(SomeRequest request)
        {
            return Task.FromResult(new SomeResponse());
        }
    }
}