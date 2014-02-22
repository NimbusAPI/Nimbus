using System;
using System.Threading.Tasks;
using Nimbus.Handlers;
using Nimbus.SampleApp.MessageContracts;

namespace Nimbus.SampleApp.Handlers
{
    public class WhatIsTheAnswerRequestHandler : IHandleRequest<WhatIsTheAnswerRequest, WhatIsTheAnswerResponse>
    {
        public async Task<WhatIsTheAnswerResponse> Handle(WhatIsTheAnswerRequest request)
        {
            Console.WriteLine("Received question: " + request.Question);

            return new WhatIsTheAnswerResponse
            {
                TheAnswer = "42",
            };
        }
    }
}