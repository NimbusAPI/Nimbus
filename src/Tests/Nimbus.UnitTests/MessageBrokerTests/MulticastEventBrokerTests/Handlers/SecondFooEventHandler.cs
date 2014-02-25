using System.Threading.Tasks;
using Nimbus.Handlers;
using Nimbus.UnitTests.MessageBrokerTests.MulticastEventBrokerTests.MessageContracts;

#pragma warning disable 4014

namespace Nimbus.UnitTests.MessageBrokerTests.MulticastEventBrokerTests.Handlers
{
    public class SecondFooEventHandler : IHandleMulticastEvent<FooEvent>
    {
        public async Task Handle(FooEvent busEvent)
        {
            MethodCallCounter.RecordCall<SecondFooEventHandler>(h => h.Handle(busEvent));
        }
    }
}