using System;
using System.Threading.Tasks;
using Nimbus.Handlers;
using Nimbus.IntegrationTests.Tests.LongRunningHandlerTests.MessageContracts;
using Nimbus.Tests.Common;

namespace Nimbus.IntegrationTests.Tests.LongRunningHandlerTests.CommandHandlers
{
    public class LongRunningCommandHandler : IHandleCommand<LongRunningCommand>, ILongRunningTask
    {
        internal static IBus Bus { get; set; }

        public async Task Handle(LongRunningCommand busCommand)
        {
            Console.WriteLine("Received LongRunningCommand @ {0}", DateTime.Now);
            MethodCallCounter.RecordCall<LongRunningCommandHandler>(ch => ch.Handle(busCommand));

            // This should cause the lock to be renewed a few times
            await Task.Delay(TimeSpan.FromSeconds(18));

            Console.WriteLine("Publishing LongRunningCommandCompletedEvent @ {0}", DateTime.Now);
            await Bus.Publish(new LongRunningCommandCompletedEvent());
        }

        public bool IsAlive
        {
            get
            {
                Console.WriteLine("Asked if LongRunningCommandHandler was still alive: Of course we are! @ {0}", DateTime.Now);
                return true;
            }
        }
    }
}