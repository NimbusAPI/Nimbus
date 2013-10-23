using System;
using System.Threading.Tasks;
using Nimbus.SampleApp.MessageContracts;

namespace Nimbus.SampleApp
{
    public class DeepThought
    {
        private readonly IBus _bus;

        public DeepThought(IBus bus)
        {
            _bus = bus;
        }

        public async Task ComputeTheAnswer()
        {
            Console.WriteLine("Thinking...");

            var response = await _bus.Request(new WhatIsTheAnswerRequest {Question = "Life, the universe and everything"});

            Console.WriteLine(response.TheAnswer);
        }
    }
}