using System.Threading.Tasks;
using Nimbus.Handlers;
using Nimbus.UnitTests.DispatcherTests.MessageContracts;

namespace Nimbus.UnitTests.DispatcherTests.Handlers
{
    public class EmptyRequestHandler : IHandleRequest<EmptyRequest, EmptyResponse>
    {
        public Task<EmptyResponse> Handle(EmptyRequest request)
        {
            return null;
        }
    }
}