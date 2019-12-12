using System.Threading.Tasks;
using Nimbus.InfrastructureContracts.Handlers;
using Nimbus.Tests.Unit.DispatcherTests.MessageContracts;

namespace Nimbus.Tests.Unit.DispatcherTests.Handlers
{
    public class EmptyEventHandler : IHandleMulticastEvent<EmptyEvent>
    {
        public Task Handle(EmptyEvent busEvent)
        {
            return null;
        }
    }
}