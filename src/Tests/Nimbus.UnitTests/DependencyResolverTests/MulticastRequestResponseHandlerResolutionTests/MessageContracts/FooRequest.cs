using Nimbus.MessageContracts;

namespace Nimbus.UnitTests.DependencyResolverTests.MulticastRequestResponseHandlerResolutionTests.MessageContracts
{
    public class FooRequest : IBusRequest<FooRequest, FooResponse>
    {
    }
}