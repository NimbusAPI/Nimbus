using System.Threading.Tasks;
using Nimbus.Handlers;
using Nimbus.UnitTests.DispatcherTests.MessageContracts;

namespace Nimbus.UnitTests.DispatcherTests.Handlers
{
    public class EmptyCommandHandler : IHandleCommand<EmptyCommand>
    {
        public Task Handle(EmptyCommand busCommand)
        {
            return null;
        }
    }
}