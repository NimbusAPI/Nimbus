using Nimbus.MessageContracts;

namespace Nimbus.UnitTests.DependencyResolverTests.AllComponentTypes.MessageContracts
{
    public class FooRequest : IBusRequest<FooRequest, FooResponse>
    {
    }
}