using Nimbus.MessageContracts;

namespace Nimbus.UnitTests.MessageBrokerTests.RequestResponseTests.MessageContracts
{
    public class FooRequest : IBusRequest<FooRequest, FooResponse>
    {
    }
}