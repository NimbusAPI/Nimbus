using System.Threading.Tasks;
using Nimbus.InfrastructureContracts.Handlers;
using Nimbus.Tests.Integration.Tests.BusStartingAndStopping.MessageContracts;

namespace Nimbus.Tests.Integration.Tests.BusStartingAndStopping.Handlers
{
    public class QuickCommandHandler : IHandleCommand<QuickCommand>
    {
        public async Task Handle(QuickCommand busCommand)
        {
        }
    }
}