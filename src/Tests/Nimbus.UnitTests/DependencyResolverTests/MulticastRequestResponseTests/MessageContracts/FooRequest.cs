using Nimbus.MessageContracts;

namespace Nimbus.UnitTests.DependencyResolverTests.MulticastRequestResponseTests.MessageContracts
{
    public class FooRequest : IBusRequest<FooRequest, FooResponse>
    {
    }
}