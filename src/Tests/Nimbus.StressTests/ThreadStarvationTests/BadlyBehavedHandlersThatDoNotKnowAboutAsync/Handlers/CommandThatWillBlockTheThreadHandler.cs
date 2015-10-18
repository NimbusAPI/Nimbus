using System;
using System.Threading;
using System.Threading.Tasks;
using Nimbus.Handlers;
using Nimbus.StressTests.ThreadStarvationTests.BadlyBehavedHandlersThatDoNotKnowAboutAsync.MessageContracts;
using Nimbus.Tests.Common;

namespace Nimbus.StressTests.ThreadStarvationTests.BadlyBehavedHandlersThatDoNotKnowAboutAsync.Handlers
{
#pragma warning disable 1998 // This async method lacks 'await' operators and will run synchronously.
#pragma warning disable 4014 // Because this call is not awaited, execution of the current method continues before the call is completed.
	public class CommandThatWillBlockTheThreadHandler : IHandleCommand<CommandThatWillBlockTheThread>, ILongRunningTask
    {
        public static readonly TimeSpan SleepDuration = TimeSpan.FromSeconds(10);

		  public async Task Handle(CommandThatWillBlockTheThread busCommand)
        {
            Thread.Sleep(SleepDuration); // deliberately block the handling thread
            MethodCallCounter.RecordCall<CommandThatWillBlockTheThreadHandler>(h => h.Handle(busCommand));
        }

        public bool IsAlive
        {
            get { return true; }
        }
    }
}