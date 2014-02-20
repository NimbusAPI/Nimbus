using System.Threading.Tasks;
using Nimbus.InfrastructureContracts;
using Nimbus.UnitTests.MessageBrokerTests.MulticastEventBrokerTests.MessageContracts;

namespace Nimbus.UnitTests.MessageBrokerTests.MulticastEventBrokerTests.Handlers
{
    public class FirstFooEventHandler : IHandleMulticastEvent<FooEvent>
    {
        public async Task Handle(FooEvent busEvent)
        {
            MethodCallCounter.RecordCall<FirstFooEventHandler>(h => h.Handle(busEvent));
        }
    }
}