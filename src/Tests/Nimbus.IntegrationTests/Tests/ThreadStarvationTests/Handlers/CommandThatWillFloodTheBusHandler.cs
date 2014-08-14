using System;
using System.Threading;
using System.Threading.Tasks;
using Nimbus.Handlers;
using Nimbus.IntegrationTests.Tests.ThreadStarvationTests.MessageContracts;
using Nimbus.Tests.Common;

namespace Nimbus.IntegrationTests.Tests.ThreadStarvationTests.Handlers
{
    public class CommandThatWillFloodTheBusHandler : IHandleCommand<CommandThatWillFloodTheBus>, ILongRunningTask
    {
        public async Task Handle(CommandThatWillFloodTheBus busCommand)
        {
            //await Task.Delay(TimeSpan.FromSeconds(15));
            Thread.Sleep(15000);
            MethodCallCounter.RecordCall<CommandThatWillFloodTheBusHandler>(h => h.Handle(busCommand));
        }

        public bool IsAlive
        {
            get { return true; }
        }
    }
}