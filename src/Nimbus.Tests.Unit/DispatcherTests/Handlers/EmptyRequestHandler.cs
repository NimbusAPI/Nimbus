using System.Threading.Tasks;
using Nimbus.InfrastructureContracts.Handlers;
using Nimbus.Tests.Unit.DispatcherTests.MessageContracts;

namespace Nimbus.Tests.Unit.DispatcherTests.Handlers
{
    public class EmptyRequestHandler : IHandleRequest<EmptyRequest, EmptyResponse>
    {
        public Task<EmptyResponse> Handle(EmptyRequest request)
        {
            return null;
        }
    }
}