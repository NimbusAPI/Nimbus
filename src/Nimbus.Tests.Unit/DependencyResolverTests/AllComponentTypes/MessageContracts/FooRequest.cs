using Nimbus.MessageContracts;

namespace Nimbus.Tests.Unit.DependencyResolverTests.AllComponentTypes.MessageContracts
{
    public class FooRequest : IBusRequest<FooRequest, FooResponse>
    {
    }
}