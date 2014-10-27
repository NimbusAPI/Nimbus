using System.Threading.Tasks;
using Nimbus.Handlers;
using Nimbus.UnitTests.ContainerRegistrationTests.MessageContracts;

namespace Nimbus.UnitTests.ContainerRegistrationTests.Handlers
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