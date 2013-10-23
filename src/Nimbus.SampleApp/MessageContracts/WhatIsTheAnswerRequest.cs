using Nimbus.MessageContracts;

namespace Nimbus.SampleApp.MessageContracts
{
    public class WhatIsTheAnswerRequest : BusRequest<WhatIsTheAnswerRequest, WhatIsTheAnswerResponse>
    {
        public string Question { get; set; }
    }
}