using System;
using System.Threading.Tasks;
using Nimbus.SampleApp.MessageContracts;
using Serilog;

namespace Nimbus.SampleApp
{
    public class DeepThought
    {
        private readonly IBus _bus;
        private readonly ILogger _logger;

        public DeepThought(IBus bus, ILogger logger)
        {
            _bus = bus;
            _logger = logger;
        }

        public async Task ComputeTheAnswer()
        {
            _logger.Information("Thinking...");

            var response = await _bus.Request(new WhatIsTheAnswerRequest {Question = "Life, the universe and everything"});

            _logger.Information("Computed answer: {answer}", response.TheAnswer);

            Console.WriteLine();
        }
    }
}