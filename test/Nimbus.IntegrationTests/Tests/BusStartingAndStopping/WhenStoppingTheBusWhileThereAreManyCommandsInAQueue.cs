﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Nimbus.Configuration;
using Nimbus.Extensions;
using Nimbus.IntegrationTests.Tests.BusStartingAndStopping.Handlers;
using Nimbus.IntegrationTests.Tests.BusStartingAndStopping.MessageContracts;
using Nimbus.Tests.Common.Extensions;
using Nimbus.Tests.Common.TestScenarioGeneration.ScenarioComposition;
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
        private int _messagesReceivedBeforeTheBusWasStopped;
        private int _messagesReceivedAfterTheBusWasStopped;

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
            _messagesReceivedBeforeTheBusWasStopped = MethodCallCounter.AllReceivedMessages.OfType<SlowCommand>().Count();
            await Bus.Stop();

            MethodCallCounter.Clear();
            SlowCommandHandler.HandlerSemaphore.Release(_totalCommandsToSend);
            await Task.Delay(TimeSpan.FromSeconds(0.25));
            _messagesReceivedAfterTheBusWasStopped = MethodCallCounter.AllReceivedMessages.OfType<SlowCommand>().Count();
        }

        [Test]
        [TestCaseSource(typeof (AllBusConfigurations<WhenStoppingTheBusWhileThereAreManyCommandsInAQueue>))]
        public async Task Run(string testName, IConfigurationScenario<BusBuilderConfiguration> scenario)
        {
            await Given(scenario);
            await When();
            await Then();
        }

        [Then]
        public async Task TheNumberOfCommandsSeenShouldBeTheNumberOfConcurrentHandlers()
        {
            // The number of commands dispatched by the bus should have been the number of concurrent handlers, after which the throttling
            // should have paused any further dispatching. Given that we only release the throttle after we've stopped the bus, the number of
            // messages dispatched should exactly match the number of concurrent handlers.
            _messagesReceivedBeforeTheBusWasStopped.ShouldBe(Instance.Configuration.ConcurrentHandlerLimit);
        }

        [Then]
        public async Task NoMoreHandlerInvocationsShouldHaveOccurredAfterTheBusWasStopped()
        {
            _messagesReceivedAfterTheBusWasStopped.ShouldBe(0);
        }
    }
}