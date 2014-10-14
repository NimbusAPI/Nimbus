using System;
using System.Threading;
using System.Threading.Tasks;
using Nimbus.Handlers;
using Nimbus.StressTests.ThreadStarvationTests.MessageContracts;
using Nimbus.Tests.Common;

namespace Nimbus.StressTests.ThreadStarvationTests.Handlers
{
    public class CommandThatWillBlockTheThreadHandler : IHandleCommand<CommandThatWillBlockTheThread>, ILongRunningTask
    {
        public async Task Handle(CommandThatWillBlockTheThread busCommand)
        {
            Thread.Sleep(TimeSpan.FromSeconds(1)); // deliberately block the handling thread
            MethodCallCounter.RecordCall<CommandThatWillBlockTheThreadHandler>(h => h.Handle(busCommand));
        }

        public bool IsAlive
        {
            get { return true; }
        }
    }
}