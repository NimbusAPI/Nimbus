using System.Threading.Tasks;
using Nimbus.Handlers;
using Nimbus.UnitTests.DispatcherTests.MessageContracts;

namespace Nimbus.UnitTests.DispatcherTests.Handlers
{
    public class EmptyEventHandler : IHandleMulticastEvent<EmptyEvent>
    {
        public Task Handle(EmptyEvent busEvent)
        {
            return null;
        }
    }
}