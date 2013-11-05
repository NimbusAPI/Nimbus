using Nimbus.MessageContracts;

namespace Nimbus.SampleApp.MessageContracts
{
    public class WhatIsTheAnswerResponse : IBusResponse
    {
        public string TheAnswer { get; set; }
    }
}