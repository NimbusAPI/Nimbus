using System.Threading.Tasks;
using Nimbus.Handlers;

namespace Nimbus.UnitTests.InfrastructureTests.Handlers
{
    public class MyLongNamedHandler : IHandleMulticastEvent<MyEventWithALongName>
    {
        public async Task Handle(MyEventWithALongName busEvent)
        {
            
        }
    }
}