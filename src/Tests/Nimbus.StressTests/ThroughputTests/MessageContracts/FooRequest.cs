using Nimbus.MessageContracts;

namespace Nimbus.StressTests.ThroughputTests.MessageContracts
{
    public class FooRequest : StressTestMessage, IBusRequest<FooRequest, FooResponse>
    {
    }
}