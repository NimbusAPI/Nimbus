using System;
using System.Linq;
using System.Threading.Tasks;
using Nimbus.Configuration;
using Nimbus.Extensions;
using Nimbus.IntegrationTests.Tests.BusStartingAndStopping.MessageContracts;
using Nimbus.Tests.Common;
using Nimbus.Tests.Common.Extensions;
using Nimbus.Tests.Common.TestScenarioGeneration;
using Nimbus.Tests.Common.TestScenarioGeneration.TestCaseSources;
using Nimbus.Tests.Common.TestUtilities;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.IntegrationTests.Tests.BusStartingAndStopping
{
    [TestFixture]
    public class WhenStoppingTheBusWhileThereAreManyCommandsInAQueue : TestForBus
    {
        private const int _totalCommands = 100;

        private int _commandHandlerInvocationCount;
        private int _additionalCommandHandlerInvocationCount;

        protected override async Task When()
        {
            Enumerable.Range(0, _totalCommands)
                      .Select(i => Bus.Send(new SlowCommand()))
                      .WaitAll();
            await TimeSpan.FromSeconds(1).WaitUntil(() => MethodCallCounter.TotalReceivedCalls > 0);
            await Bus.Stop();

            _commandHandlerInvocationCount = MethodCallCounter.AllReceivedMessages.OfType<SlowCommand>().Count();
            Console.WriteLine("Bus has stopped.");
            Console.WriteLine("Number of commands received immediately afterwards: {0}", _commandHandlerInvocationCount);
            MethodCallCounter.Clear();
        }

        [Test]
        [TestCaseSource(typeof (AllBusConfigurations<WhenStoppingTheBusWhileThereAreManyCommandsInAQueue>))]
        public async Task TheBusShouldStopBeforeAllTheCommandsAreHandled(string testName, BusBuilderConfiguration busBuilderConfiguration)
        {
            await Given(busBuilderConfiguration);
            await When();

            Console.WriteLine("Observed a total of {0} command handler invocations", _commandHandlerInvocationCount);
            _commandHandlerInvocationCount.ShouldBeLessThan(_totalCommands);
        }

        [Test]
        [TestCaseSource(typeof (AllBusConfigurations<WhenStoppingTheBusWhileThereAreManyCommandsInAQueue>))]
        public async Task AtLeastSomeOfTheCommandsShouldHaveBeenHandled(string testName, BusBuilderConfiguration busBuilderConfiguration)
        {
            await Given(busBuilderConfiguration);
            await When();

            _commandHandlerInvocationCount.ShouldBeGreaterThan(0);
        }

        [Test]
        [TestCaseSource(typeof (AllBusConfigurations<WhenStoppingTheBusWhileThereAreManyCommandsInAQueue>))]
        public async Task NoMoreHandlerInvocationsShouldHaveOccurredAfterTheBusWasStopped(string testName, BusBuilderConfiguration busBuilderConfiguration)
        {
            await Given(busBuilderConfiguration);
            await When();

            await Task.Delay(TimeSpan.FromSeconds(0.5));
            MethodCallCounter.Stop();
            _additionalCommandHandlerInvocationCount = MethodCallCounter.AllReceivedMessages.OfType<SlowCommand>().Count();
            Console.WriteLine("Number of commands received after that: {0}", _additionalCommandHandlerInvocationCount);

            _additionalCommandHandlerInvocationCount.ShouldBe(0);
        }
    }
}