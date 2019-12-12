using System.Threading.Tasks;
using Nimbus.InfrastructureContracts.Handlers;
using Nimbus.Tests.Unit.ContainerRegistrationTests.MessageContracts;

namespace Nimbus.Tests.Unit.ContainerRegistrationTests.Handlers
{
    public class HandlerThatHandlesMultipleCommandTypes : IHandleCommand<FooCommand>, IHandleCommand<BarCommand>
    {
        public async Task Handle(FooCommand busCommand)
        {
        }

        public async Task Handle(BarCommand busCommand)
        {
        }
    }
}