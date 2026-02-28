using System.Threading.Tasks;
using Nimbus.InfrastructureContracts.Handlers;
using Nimbus.Tests.Integration.Tests.AuditingInterceptorTests.MessageTypes;

namespace Nimbus.Tests.Integration.Tests.AuditingInterceptorTests.Handlers
{
    public class SomeMulticastRequestHandler : IHandleMulticastRequest<SomeMulticastRequest, SomeMulticastResponse>
    {
        public Task<SomeMulticastResponse> Handle(SomeMulticastRequest request)
        {
            return Task.FromResult(new SomeMulticastResponse());
        }
    }
}