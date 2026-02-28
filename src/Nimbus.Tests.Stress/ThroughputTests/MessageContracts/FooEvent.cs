using Nimbus.MessageContracts;

namespace Nimbus.StressTests.ThroughputTests.MessageContracts
{
    public class FooEvent : StressTestMessage, IBusEvent
    {
    }
}