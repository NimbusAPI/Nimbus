using System;
using System.Linq;
using System.Threading.Tasks;
using Nimbus.Configuration;
using Nimbus.Extensions;
using Nimbus.IntegrationTests.Tests.BusStartingAndStopping.MessageContracts;
using Nimbus.IntegrationTests.TestScenarioGeneration;
using Nimbus.Tests.Common;
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
            await Task.Delay(TimeSpan.FromMilliseconds(500));
            await Bus.Stop();

            _commandHandlerInvocationCount = MethodCallCounter.AllReceivedMessages.OfType<SlowCommand>().Count();
            Console.WriteLine("Bus has stopped.");
            Console.WriteLine("Number of commands received immediately afterwards: {0}", _commandHandlerInvocationCount);
            MethodCallCounter.Clear();

            await Task.Delay(TimeSpan.FromSeconds(2));
            _additionalCommandHandlerInvocationCount = MethodCallCounter.AllReceivedMessages.OfType<SlowCommand>().Count();
            Console.WriteLine("Number of commands received after that: {0}", _additionalCommandHandlerInvocationCount);
            MethodCallCounter.Stop();
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

            _additionalCommandHandlerInvocationCount.ShouldBe(0);
        }
    }
}