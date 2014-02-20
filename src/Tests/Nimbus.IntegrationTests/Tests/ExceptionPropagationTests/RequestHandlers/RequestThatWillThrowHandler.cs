using System.Diagnostics;
using System.Threading.Tasks;
using Nimbus.InfrastructureContracts;
using Nimbus.IntegrationTests.Tests.ExceptionPropagationTests.MessageContracts;

namespace Nimbus.IntegrationTests.Tests.ExceptionPropagationTests.RequestHandlers
{
    public class RequestThatWillThrowHandler : IHandleRequest<RequestThatWillThrow, RequestThatWillThrowResponse>
    {
        public const string ExceptionMessage = "This is supposed to go bang.";

        [DebuggerStepThrough]
        public async Task<RequestThatWillThrowResponse> Handle(RequestThatWillThrow request)
        {
            throw new DemonstrationException(ExceptionMessage);
        }
    }
}