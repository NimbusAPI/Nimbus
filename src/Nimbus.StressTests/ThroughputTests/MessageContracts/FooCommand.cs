using Nimbus.MessageContracts;

namespace Nimbus.StressTests.ThroughputTests.MessageContracts
{
    public class FooCommand : IBusCommand
    {
        public string SomeMessage { get; set; }
    }
}