using System;
using System.Linq;
using System.Threading.Tasks;
using Nimbus.Configuration;
using Nimbus.Extensions;
using Nimbus.Tests.Common.Extensions;
using Nimbus.Tests.Integration.Tests.BusStartingAndStopping.MessageContracts;
using Nimbus.Tests.Integration.TestScenarioGeneration.ScenarioComposition;
using Nimbus.Tests.Integration.TestScenarioGeneration.TestCaseSources;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.Tests.Integration.Tests.BusStartingAndStopping
{
    [TestFixture]
    //[Timeout(TimeoutSeconds*1000)]
    [Parallelizable(ParallelScope.None)]
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

            await Timeout.WaitUntil(() => MethodCallCounter.AllReceivedMessages.OfType<SlowCommand>().Any());

            await HaveYouTriedTurningItOffAndOnAgain();
            await HaveYouTriedTurningItOffAndOnAgain();
            await HaveYouTriedTurningItOffAndOnAgain();

            await Timeout.WaitUntil(AllOfTheCommandsThatWereSentHaveBeenReceivedAtLeastOnceByTheHandler);

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
        [TestCaseSource(typeof(AllBusConfigurations<WhenStartingAndStoppingABusMultipleTimesWhileASlowHandlerIsRunning>))]
        public async Task Run(string testName, IConfigurationScenario<BusBuilderConfiguration> scenario)
        {
            await Given(scenario);
            await When();
            await Then();
        }

        [Then]
        public async Task AllOfTheCommandsThatWereSentShouldHaveBeenReceivedAtLeastOnceByTheHandler()
        {
            AllOfTheCommandsThatWereSentHaveBeenReceivedAtLeastOnceByTheHandler().ShouldBe(true);
        }

        private bool AllOfTheCommandsThatWereSentHaveBeenReceivedAtLeastOnceByTheHandler()
        {
            return MethodCallCounter.AllReceivedMessages.OfType<SlowCommand>()
                                    .All(c => _sentCommandIds.Contains(c.SomeId));
        }
    }
}