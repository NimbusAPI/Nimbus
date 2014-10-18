using System.Threading.Tasks;
using Nimbus.Handlers;
using Nimbus.UnitTests.InfrastructureTests.MessageContracts;

namespace Nimbus.UnitTests.InfrastructureTests.Handlers
{
    public class MyLongNamedHandler : IHandleMulticastEvent<MyEventWithALongName>
    {
        public async Task Handle(MyEventWithALongName busEvent)
        {
            
        }
    }
}