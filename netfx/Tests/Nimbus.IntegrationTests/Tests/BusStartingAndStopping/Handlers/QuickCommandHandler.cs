using System.Threading.Tasks;
using Nimbus.Handlers;
using Nimbus.IntegrationTests.Tests.BusStartingAndStopping.MessageContracts;

namespace Nimbus.IntegrationTests.Tests.BusStartingAndStopping.Handlers
{
    public class QuickCommandHandler : IHandleCommand<QuickCommand>
    {
        public async Task Handle(QuickCommand busCommand)
        {
        }
    }
}