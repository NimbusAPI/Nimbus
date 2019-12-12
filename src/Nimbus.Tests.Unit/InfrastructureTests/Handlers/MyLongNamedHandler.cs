using System.Threading.Tasks;
using Nimbus.InfrastructureContracts.Handlers;
using Nimbus.Tests.Unit.InfrastructureTests.MessageContracts;

namespace Nimbus.Tests.Unit.InfrastructureTests.Handlers
{
    public class MyLongNamedHandler : IHandleMulticastEvent<MyEventWithALongName>
    {
        public async Task Handle(MyEventWithALongName busEvent)
        {
            
        }
    }
}