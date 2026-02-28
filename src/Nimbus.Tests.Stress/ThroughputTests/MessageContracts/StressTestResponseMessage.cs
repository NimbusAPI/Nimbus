using System;

namespace Nimbus.StressTests.ThroughputTests.MessageContracts
{
    public class StressTestResponseMessage : StressTestMessage
    {
        public DateTimeOffset RequestSentAt { get; set; }
    }
}