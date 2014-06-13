using System;
using System.Threading.Tasks;
using Nimbus.Handlers;
using Nimbus.UnitTests.DispatcherTests.MessageContracts;

namespace Nimbus.UnitTests.DispatcherTests.Handlers
{
    public class ExceptingCommandHandler : IHandleCommand<ExceptingCommand>
    {
        public Task Handle(ExceptingCommand busCommand)
        {
            throw new Exception("Ruh roh");
        }
    }
}