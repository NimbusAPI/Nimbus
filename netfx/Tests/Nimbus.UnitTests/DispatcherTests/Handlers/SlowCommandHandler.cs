using System.Threading;
using System.Threading.Tasks;
using Nimbus.Handlers;
using Nimbus.UnitTests.DispatcherTests.MessageContracts;

namespace Nimbus.UnitTests.DispatcherTests.Handlers
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