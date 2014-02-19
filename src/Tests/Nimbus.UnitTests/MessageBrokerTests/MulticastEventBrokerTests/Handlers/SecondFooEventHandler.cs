using Nimbus.InfrastructureContracts;
using Nimbus.UnitTests.MessageBrokerTests.MulticastEventBrokerTests.MessageContracts;

namespace Nimbus.UnitTests.MessageBrokerTests.MulticastEventBrokerTests.Handlers
{
    public class SecondFooEventHandler : IHandleMulticastEvent<FooEvent>
    {
        public void Handle(FooEvent busEvent)
        {
            MethodCallCounter.RecordCall<SecondFooEventHandler>(h => h.Handle(busEvent));
        }
    }
}