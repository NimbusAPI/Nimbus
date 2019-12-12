using System;

namespace Nimbus.StressTests.ThroughputTests.MessageContracts
{
    public class StressTestMessage
    {
        public DateTimeOffset WhenSent { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset WhenReceived { get; set; }
    }
}