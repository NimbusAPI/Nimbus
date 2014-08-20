using System;
using System.Threading.Tasks;
using Nimbus.Handlers;
using Nimbus.IntegrationTests.Tests.LongRunningHandlerTests.MessageContracts;
using Nimbus.Tests.Common;

namespace Nimbus.IntegrationTests.Tests.LongRunningHandlerTests.CommandHandlers
{
    public class LongRunningEventHandler : IHandleCompetingEvent<LongRunningCommandCompletedEvent>
    {
        public async Task Handle(LongRunningCommandCompletedEvent busEvent)
        {
            Console.WriteLine("Received LongRunningCommandCompletedEvent @ {0}", DateTime.Now);
            MethodCallCounter.RecordCall<LongRunningEventHandler>(ch => ch.Handle(busEvent));
        }
    }
}