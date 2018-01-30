using System;
using System.Threading.Tasks;
using Nimbus.Handlers;
using Nimbus.IntegrationTests.Tests.BusBuilderTests.MessageContracts;

namespace Nimbus.IntegrationTests.Tests.BusBuilderTests.Handlers
{
    public class SomeCommandHandler : IHandleCommand<SomeCommand>
    {
        public Task Handle(SomeCommand busCommand)
        {
            throw new NotImplementedException();
        }
    }
}