using System;
using System.Linq;
using System.Threading.Tasks;
using Nimbus.Configuration;
using Nimbus.Extensions;
using Nimbus.IntegrationTests.Tests.BusStartingAndStopping.Handlers;
using Nimbus.IntegrationTests.Tests.BusStartingAndStopping.MessageContracts;
using Nimbus.Tests.Common.Extensions;
using Nimbus.Tests.Common.TestScenarioGeneration.ConfigurationSources;
using Nimbus.Tests.Common.TestScenarioGeneration.TestCaseSources;
using Nimbus.Tests.Common.TestUtilities;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.IntegrationTests.Tests.BusStartingAndStopping
{
    [TestFixture]
    public class WhenStoppingTheBusWhileThereAreManyCommandsInAQueue : TestForBus
    {
        private int _totalCommandsToSend;
        private int _expectedNumberOfCommandsToBeSeenBeforeTheBusStops;

        protected override async Task When()
        {
            var overallConcurrentHandlerLimit = Math.Min(Instance.Configuration.ConcurrentHandlerLimit.Value, Instance.Configuration.GlobalConcurrentHandlerLimit.Value);

            _totalCommandsToSend = overallConcurrentHandlerLimit*2;
            _expectedNumberOfCommandsToBeSeenBeforeTheBusStops = overallConcurrentHandlerLimit;

            SlowCommandHandler.Reset();

            await Enumerable.Range(0, _totalCommandsToSend)
                            .Select(i => Bus.Send(new SlowCommand()))
                            .WhenAll();

            await TimeSpan.FromSeconds(TimeoutSeconds)
                          .WaitUntil(() => MethodCallCounter.AllReceivedMessages.OfType<SlowCommand>().Count() >= _expectedNumberOfCommandsToBeSeenBeforeTheBusStops);
            await Bus.Stop();
            Console.WriteLine("Bus has stopped.");
        }

        [Test]
        [TestCaseSource(typeof (AllBusConfigurations<WhenStoppingTheBusWhileThereAreManyCommandsInAQueue>))]
        public async Task TheNumberOfCommandsSeenShouldBeTheNumberOfConcurrentHandlers(string testName, IConfigurationScenario<BusBuilderConfiguration> scenario)
        {
            await Given(scenario);
            await When();

            // The number of commands dispatched by the bus should have been the number of concurrent handlers, after which the throttling
            // should have paused any further dispatching. Given that we only release the throttle after we've stopped the bus, the number of
            // messages dispatched should exactly match the number of concurrent handlers.
            MethodCallCounter.AllReceivedMessages.OfType<SlowCommand>().Count().ShouldBe(Instance.Configuration.ConcurrentHandlerLimit);
        }

        [Test]
        [TestCaseSource(typeof (AllBusConfigurations<WhenStoppingTheBusWhileThereAreManyCommandsInAQueue>))]
        public async Task NoMoreHandlerInvocationsShouldHaveOccurredAfterTheBusWasStopped(string testName, IConfigurationScenario<BusBuilderConfiguration> scenario)
        {
            await Given(scenario);
            await When();

            MethodCallCounter.Clear();
            SlowCommandHandler.HandlerSemaphore.Release(_totalCommandsToSend);
            await Task.Delay(TimeSpan.FromSeconds(0.25));

            MethodCallCounter.AllReceivedMessages.OfType<SlowCommand>().Count().ShouldBe(0);
        }
    }
}