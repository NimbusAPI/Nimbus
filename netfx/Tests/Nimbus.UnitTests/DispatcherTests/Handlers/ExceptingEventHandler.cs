using System;
using System.Threading.Tasks;
using Nimbus.Handlers;
using Nimbus.UnitTests.DispatcherTests.MessageContracts;

namespace Nimbus.UnitTests.DispatcherTests.Handlers
{
    public class ExceptingEventHandler : IHandleMulticastEvent<ExceptingEvent>
    {
        public Task Handle(ExceptingEvent busEvent)
        {
            throw new Exception("Ruh roh");
        }
    }
}