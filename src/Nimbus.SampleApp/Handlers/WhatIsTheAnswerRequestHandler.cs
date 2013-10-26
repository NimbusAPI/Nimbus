using System;
using Nimbus.InfrastructureContracts;
using Nimbus.SampleApp.MessageContracts;
using Serilog;

namespace Nimbus.SampleApp.Handlers
{
    public class WhatIsTheAnswerRequestHandler : IHandleRequest<WhatIsTheAnswerRequest, WhatIsTheAnswerResponse>
    {
        private readonly ILogger _logger;

        public WhatIsTheAnswerRequestHandler(ILogger logger)
        {
            _logger = logger;
        }

        public WhatIsTheAnswerResponse Handle(WhatIsTheAnswerRequest request)
        {
            _logger.Information("Received question: {question}", request.Question);

            return new WhatIsTheAnswerResponse
            {
                TheAnswer = "42",
            };
        }
    }
}