using System.Threading.Tasks;
using Nimbus.InfrastructureContracts.Handlers;
using Nimbus.Tests.Integration.Tests.RequestResponseCorrelationTests.MessageContracts;

namespace Nimbus.Tests.Integration.Tests.RequestResponseCorrelationTests.RequestHandlers
{
    public class FastRequestHandler : IHandleRequest<FastRequest, FastResponse>
    {
        public Task<FastResponse> Handle(FastRequest request)
        {
            return Task.FromResult(new FastResponse());
        }
    }
}
