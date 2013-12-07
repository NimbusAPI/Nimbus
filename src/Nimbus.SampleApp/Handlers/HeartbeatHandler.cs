using System;
using Nimbus.InfrastructureContracts;
using Nimbus.SampleApp.MessageContracts;

namespace Nimbus.SampleApp.Handlers
{
    public class HeartbeatHandler : IHandleMulticastEvent<HeartbeatEvent>
    {
        public void Handle(HeartbeatEvent busEvent)
        {
            Console.WriteLine("I'm still alive");
        }
    }
}