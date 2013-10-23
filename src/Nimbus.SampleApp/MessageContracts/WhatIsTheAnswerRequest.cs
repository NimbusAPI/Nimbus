using Nimbus.MessageContracts;

namespace Nimbus.SampleApp
{
    public class WhatIsTheAnswerRequest : BusRequest<WhatIsTheAnswerRequest, WhatIsTheAnswerResponse>
    {
        public string Question { get; set; }
    }
}