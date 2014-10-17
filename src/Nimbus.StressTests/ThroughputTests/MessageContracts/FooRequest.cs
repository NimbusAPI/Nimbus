using Nimbus.MessageContracts;

namespace Nimbus.StressTests.ThroughputTests.MessageContracts
{
    public class FooRequest : IBusRequest<FooRequest, FooResponse>
    {
    }
}