using System.Threading.Tasks;
using Nimbus.InfrastructureContracts;
using Nimbus.UnitTests.MessageBrokerTests.CompetingEventBrokerTests.MessageContracts;

#pragma warning disable 4014

namespace Nimbus.UnitTests.MessageBrokerTests.CompetingEventBrokerTests.Handlers
{
    public class FirstFooEventHandler : IHandleCompetingEvent<FooEvent>
    {
        public async Task Handle(FooEvent busEvent)
        {
            MethodCallCounter.RecordCall<FirstFooEventHandler>(h => h.Handle(busEvent));
        }
    }
}