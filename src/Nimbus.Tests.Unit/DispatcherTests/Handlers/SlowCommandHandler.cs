using System.Threading;
using System.Threading.Tasks;
using Nimbus.InfrastructureContracts.Handlers;
using Nimbus.Tests.Unit.DispatcherTests.MessageContracts;

namespace Nimbus.Tests.Unit.DispatcherTests.Handlers
{
    public class SlowCommandHandler : IHandleCommand<SlowCommand>
    {
        public readonly Semaphore PretendToBeWorkingSemaphore = new Semaphore(0, 1);

        public Task Handle(SlowCommand busCommand)
        {
            return Task.Run(() => { PretendToBeWorkingSemaphore.WaitOne(); });
        }
    }
}