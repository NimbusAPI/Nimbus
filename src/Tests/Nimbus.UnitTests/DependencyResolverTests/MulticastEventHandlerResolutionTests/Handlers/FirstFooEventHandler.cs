using System.Threading.Tasks;
using Nimbus.Handlers;
using Nimbus.UnitTests.DependencyResolverTests.MulticastEventHandlerResolutionTests.MessageContracts;

#pragma warning disable 4014

namespace Nimbus.UnitTests.DependencyResolverTests.MulticastEventHandlerResolutionTests.Handlers
{
    public class FirstFooEventHandler : IHandleMulticastEvent<FooEvent>
    {
        public async Task Handle(FooEvent busEvent)
        {
            MethodCallCounter.RecordCall<FirstFooEventHandler>(h => h.Handle(busEvent));
        }
    }
}