using System;
using System.Threading.Tasks;
using Nimbus.InfrastructureContracts.Handlers;
using Nimbus.Tests.Unit.DispatcherTests.MessageContracts;

namespace Nimbus.Tests.Unit.DispatcherTests.Handlers
{
    public class ExceptingEventHandler : IHandleMulticastEvent<ExceptingEvent>
    {
        public Task Handle(ExceptingEvent busEvent)
        {
            throw new Exception("Ruh roh");
        }
    }
}