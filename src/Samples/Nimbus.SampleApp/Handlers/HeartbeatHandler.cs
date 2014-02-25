using System;
using System.Threading.Tasks;
using Nimbus.Handlers;
using Nimbus.SampleApp.MessageContracts;

namespace Nimbus.SampleApp.Handlers
{
    public class HeartbeatHandler : IHandleMulticastEvent<HeartbeatEvent>
    {
        public async Task Handle(HeartbeatEvent busEvent)
        {
            Console.WriteLine("I'm still alive");
        }
    }
}