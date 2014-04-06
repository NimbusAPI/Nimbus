using Nimbus.MessageContracts;

namespace Nimbus.IntegrationTests.Tests.ThroughputTests.MessageContracts
{
    public class FooRequest : IBusRequest<FooRequest, FooResponse>
    {
    }
}