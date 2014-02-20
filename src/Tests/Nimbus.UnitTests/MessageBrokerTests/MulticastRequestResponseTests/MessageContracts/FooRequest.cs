using Nimbus.MessageContracts;

namespace Nimbus.UnitTests.MessageBrokerTests.MulticastRequestResponseTests.MessageContracts
{
    public class FooRequest : IBusRequest<FooRequest, FooResponse>
    {
    }
}