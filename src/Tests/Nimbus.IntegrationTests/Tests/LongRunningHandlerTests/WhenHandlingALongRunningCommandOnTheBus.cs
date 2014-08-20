using System;
using System.Linq;
using System.Threading.Tasks;
using Nimbus.IntegrationTests.Tests.LongRunningHandlerTests.CommandHandlers;
using Nimbus.IntegrationTests.Tests.LongRunningHandlerTests.MessageContracts;
using Nimbus.Tests.Common;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.IntegrationTests.Tests.LongRunningHandlerTests
{
    [TestFixture]
    [Timeout(60*1000)]
    public class WhenHandlingALongRunningCommandOnTheBus : TestForBus
    {
        protected override async Task When()
        {
            // NOTE: This test assumes the DefaultMessageLockDuration is 10secs
            LongRunningCommandHandler.Bus = Bus;

            var someCommand = new LongRunningCommand();
            Console.WriteLine("Sending LongRunningCommand @ {0}", DateTime.Now);
            await Bus.Send(someCommand);
            await TimeSpan.FromMinutes(1.5).WaitUntil(() => MethodCallCounter.AllReceivedMessages.Count() == 2);
            Console.WriteLine("Completing test @ {0}", DateTime.Now);
        }

        [Test]
        public async Task TheCommandHandlerShouldReceiveThatCommandAndStartProcessing()
        {
            MethodCallCounter.AllReceivedMessages.OfType<LongRunningCommand>().Count().ShouldBe(1);
        }
        
        [Test]
        public async Task TheCommandHandlerShouldGetCalledOnce()
        {
            var calls = MethodCallCounter.ReceivedCallsWithAnyArg<LongRunningCommandHandler>(h => h.Handle(null));
            calls.Count().ShouldBe(1);
        }

        [Test]
        public async Task TheEventHandlerShouldHaveBeenNotifiedTheLongRunningTaskSucceeded()
        {
            MethodCallCounter.AllReceivedMessages.OfType<LongRunningCommandCompletedEvent>().Count().ShouldBe(1);
        }

        [Test]
        public async Task TheEventHandlerShouldGetCalledOnce()
        {
            var calls = MethodCallCounter.ReceivedCallsWithAnyArg<LongRunningEventHandler>(h => h.Handle(null));
            calls.Count().ShouldBe(1);
        }
    }
}