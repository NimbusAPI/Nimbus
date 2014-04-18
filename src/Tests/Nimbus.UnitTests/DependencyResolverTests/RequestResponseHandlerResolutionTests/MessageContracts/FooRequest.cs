using Nimbus.MessageContracts;

namespace Nimbus.UnitTests.DependencyResolverTests.RequestResponseHandlerResolutionTests.MessageContracts
{
    public class FooRequest : IBusRequest<FooRequest, FooResponse>
    {
    }
}