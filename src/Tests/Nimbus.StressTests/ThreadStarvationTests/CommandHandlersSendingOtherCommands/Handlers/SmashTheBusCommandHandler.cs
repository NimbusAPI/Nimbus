using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Nimbus.Handlers;
using Nimbus.PropertyInjection;
using Nimbus.StressTests.ThreadStarvationTests.CommandHandlersSendingOtherCommands.MessageContracts;

namespace Nimbus.StressTests.ThreadStarvationTests.CommandHandlersSendingOtherCommands.Handlers
{
    public class SmashTheBusCommandHandler: IHandleCommand<SmashTheBusCommand>, ILongRunningTask, IRequireBus
    {
        public static int NumCommandsSent = 0;
        public static TimeSpan HammerTheBusFor = TimeSpan.FromSeconds(20);

        public IBus Bus { get; set; }

        public async Task Handle(SmashTheBusCommand busCommand)
        {
            const int batchSize = 10;

            var sw = Stopwatch.StartNew();
            while (sw.Elapsed < HammerTheBusFor)
            {
                var commands = Enumerable.Range(0, batchSize)
                    .Select(i => new NoOpCommand())
                    .ToArray();
                await Bus.SendAll(commands);
                NumCommandsSent += batchSize;
            }
        }

        public bool IsAlive { get { return true; } }
    }
}