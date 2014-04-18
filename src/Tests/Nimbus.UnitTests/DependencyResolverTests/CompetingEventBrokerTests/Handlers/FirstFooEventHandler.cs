using System.Threading.Tasks;
using Nimbus.Handlers;
using Nimbus.UnitTests.DependencyResolverTests.CompetingEventBrokerTests.MessageContracts;

#pragma warning disable 4014

namespace Nimbus.UnitTests.DependencyResolverTests.CompetingEventBrokerTests.Handlers
{
    public class FirstFooEventHandler : IHandleCompetingEvent<FooEvent>
    {
        public async Task Handle(FooEvent busEvent)
        {
            MethodCallCounter.RecordCall<FirstFooEventHandler>(h => h.Handle(busEvent));
        }
    }
}