using System.Threading.Tasks;
using Nimbus.InfrastructureContracts.Handlers;
using Nimbus.Tests.Unit.DispatcherTests.MessageContracts;

namespace Nimbus.Tests.Unit.DispatcherTests.Handlers
{
    public class EmptyCommandHandler : IHandleCommand<EmptyCommand>
    {
        public Task Handle(EmptyCommand busCommand)
        {
            return null;
        }
    }
}