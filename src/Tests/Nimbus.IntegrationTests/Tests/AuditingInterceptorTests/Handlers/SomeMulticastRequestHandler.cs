using System.Threading.Tasks;
using Nimbus.Handlers;
using Nimbus.IntegrationTests.Tests.AuditingInterceptorTests.MessageTypes;

namespace Nimbus.IntegrationTests.Tests.AuditingInterceptorTests.Handlers
{
    public class SomeMulticastRequestHandler : IHandleMulticastRequest<SomeMulticastRequest, SomeMulticastResponse>
    {
        public Task<SomeMulticastResponse> Handle(SomeMulticastRequest request)
        {
            return Task.FromResult(new SomeMulticastResponse());
        }
    }
}