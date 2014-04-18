using Nimbus.MessageContracts;

namespace Nimbus.UnitTests.DependencyResolverTests.RequestResponseTests.MessageContracts
{
    public class FooRequest : IBusRequest<FooRequest, FooResponse>
    {
    }
}