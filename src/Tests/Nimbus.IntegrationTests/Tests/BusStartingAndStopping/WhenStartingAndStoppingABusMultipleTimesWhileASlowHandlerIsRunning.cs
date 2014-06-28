using System;
using System.Linq;
using System.Threading.Tasks;
using Nimbus.Extensions;
using Nimbus.IntegrationTests.Extensions;
using Nimbus.IntegrationTests.Tests.BusBuilderTests.MessageContracts;
using Nimbus.IntegrationTests.Tests.BusStartingAndStopping.MessageContracts;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.IntegrationTests.Tests.BusStartingAndStopping
{
    [TestFixture]
    [Timeout(TimeoutSeconds*1000)]
    public class WhenStartingAndStoppingABusMultipleTimesWhileASlowHandlerIsRunning : TestForBus
    {
        public const int TimeoutSeconds = 20;
        private const int _totalCommands = 100;

        protected override async Task When()
        {
            Enumerable.Range(0, _totalCommands)
                      .Select(i => Bus.Send(new SlowCommand()))
                      .WaitAll();
            TimeSpan.FromSeconds(TimeoutSeconds).SleepUntil(() => MethodCallCounter.AllReceivedMessages.OfType<SomeCommand>().Any());

            await HaveYouTriedTurningItOffAndOnAgain();
            await HaveYouTriedTurningItOffAndOnAgain();
            await HaveYouTriedTurningItOffAndOnAgain();

            TimeSpan.FromSeconds(TimeoutSeconds).SleepUntil(() => MethodCallCounter.AllReceivedMessages.OfType<SomeCommand>().Count() >= _totalCommands);

            await Bus.Stop();
        }

        private async Task HaveYouTriedTurningItOffAndOnAgain()
        {
            await Task.Delay(TimeSpan.FromMilliseconds(100));
            await Bus.Stop();

            await Task.Delay(TimeSpan.FromMilliseconds(100));
            await Bus.Start();
        }

        [Test]
        public async Task NothingShouldGoBang()
        {
        }

        [Test]
        public async Task TheCorrectNumberOfCommandsShouldHaveBeenHandled()
        {
            MethodCallCounter.AllReceivedMessages.OfType<SlowCommand>().Count().ShouldBe(_totalCommands);
        }
    }
}