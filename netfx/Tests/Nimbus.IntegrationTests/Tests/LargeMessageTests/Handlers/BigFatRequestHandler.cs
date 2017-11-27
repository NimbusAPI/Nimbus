using System.Linq;
using System.Threading.Tasks;
using Nimbus.Handlers;
using Nimbus.IntegrationTests.Tests.LargeMessageTests.MessageContracts;

namespace Nimbus.IntegrationTests.Tests.LargeMessageTests.Handlers
{
    public class BigFatRequestHandler : IHandleRequest<BigFatRequest, BigFatResponse>
    {
        public const int MessageSize = 256*1024;

        public async Task<BigFatResponse> Handle(BigFatRequest request)
        {
            var bigAnswer = new string(Enumerable.Range(0, MessageSize).Select(i => '.').ToArray());

            return new BigFatResponse
                   {
                       SomeBigAnswer = bigAnswer,
                   };
        }
    }
}