using System;
using System.Threading.Tasks;
using Nimbus.InfrastructureContracts.Handlers;
using Nimbus.Tests.Integration.Tests.BusBuilderTests.MessageContracts;

namespace Nimbus.Tests.Integration.Tests.BusBuilderTests.Handlers
{
    public class SomeCommandHandler : IHandleCommand<SomeCommand>
    {
        public Task Handle(SomeCommand busCommand)
        {
            throw new NotImplementedException();
        }
    }
}