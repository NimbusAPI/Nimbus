using System;
using System.Threading.Tasks;
using Nimbus.InfrastructureContracts.Handlers;
using Nimbus.Tests.Unit.DispatcherTests.MessageContracts;

namespace Nimbus.Tests.Unit.DispatcherTests.Handlers
{
    public class ExceptingCommandHandler : IHandleCommand<ExceptingCommand>
    {
        public Task Handle(ExceptingCommand busCommand)
        {
            throw new Exception("Ruh roh");
        }
    }
}