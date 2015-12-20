using Nimbus.MessageContracts;

namespace Nimbus.StressTests.ThroughputTests.MessageContracts
{
    public class FooCommand : StressTestMessage, IBusCommand
    {
        public string SomeMessage { get; set; }
    }
}