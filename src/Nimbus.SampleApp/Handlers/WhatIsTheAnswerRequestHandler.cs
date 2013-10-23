using System;
using Nimbus.InfrastructureContracts;

namespace Nimbus.SampleApp.Handlers
{
    public class WhatIsTheAnswerRequestHandler : IHandleRequest<WhatIsTheAnswerRequest, WhatIsTheAnswerResponse>
    {
        public WhatIsTheAnswerResponse Handle(WhatIsTheAnswerRequest request)
        {
            Console.WriteLine("Received question: " + request.Question);

            return new WhatIsTheAnswerResponse
            {
                TheAnswer = "42",
            };
        }
    }
}