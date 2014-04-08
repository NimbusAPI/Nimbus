using Nimbus.MessageContracts;

namespace Nimbus.IntegrationTests.Tests.ThroughputTests.MessageContracts
{
    public class FooCommand : IBusCommand
    {
        public string SomeMessage { get; set; }
    }
}