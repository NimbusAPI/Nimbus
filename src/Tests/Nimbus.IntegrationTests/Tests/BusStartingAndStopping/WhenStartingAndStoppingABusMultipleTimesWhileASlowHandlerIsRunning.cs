using System;
using System.Linq;
using System.Threading.Tasks;
using Nimbus.Configuration;
using Nimbus.Extensions;
using Nimbus.IntegrationTests.Tests.BusStartingAndStopping.MessageContracts;
using Nimbus.Tests.Common.Extensions;
using Nimbus.Tests.Common.TestScenarioGeneration.TestCaseSources;
using Nimbus.Tests.Common.TestUtilities;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.IntegrationTests.Tests.BusStartingAndStopping
{
    [TestFixture]
    [Timeout(TimeoutSeconds*1000)]
    public class WhenStartingAndStoppingABusMultipleTimesWhileASlowHandlerIsRunning : TestForBus
    {
        public new const int TimeoutSeconds = 60;

        private SlowCommand[] _commands;
        private Guid[] _sentCommandIds;
        private const int _totalCommands = 50;

        protected override async Task When()
        {
            _commands = Enumerable.Range(0, _totalCommands)
                                  .Select(i => new SlowCommand(Guid.NewGuid()))
                                  .ToArray();

            _commands
                .Select(c => Bus.Send(c))
                .WaitAll();

            _sentCommandIds = _commands.Select(c => c.SomeId).ToArray();

            await TimeSpan.FromSeconds(TimeoutSeconds).WaitUntil(() => MethodCallCounter.AllReceivedMessages.OfType<SlowCommand>().Any());

            await HaveYouTriedTurningItOffAndOnAgain();
            await HaveYouTriedTurningItOffAndOnAgain();
            await HaveYouTriedTurningItOffAndOnAgain();

            await TimeSpan.FromSeconds(TimeoutSeconds).WaitUntil(AllOfTheCommandsThatWereSentHaveBeenReceivedAtLeastOnceByTheHandler);

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
        [TestCaseSource(typeof (AllBusConfigurations<WhenStartingAndStoppingABusMultipleTimesWhileASlowHandlerIsRunning>))]
        public async Task AllOfTheCommandsThatWereSentShouldHaveBeenReceivedAtLeastOnceByTheHandler(string testName, BusBuilderConfiguration busBuilderConfiguration)
        {
            await Given(busBuilderConfiguration);
            await When();

            AllOfTheCommandsThatWereSentHaveBeenReceivedAtLeastOnceByTheHandler().ShouldBe(true);
        }

        private bool AllOfTheCommandsThatWereSentHaveBeenReceivedAtLeastOnceByTheHandler()
        {
            return MethodCallCounter.AllReceivedMessages.OfType<SlowCommand>()
                                    .All(c => _sentCommandIds.Contains(c.SomeId));
        }
    }
}