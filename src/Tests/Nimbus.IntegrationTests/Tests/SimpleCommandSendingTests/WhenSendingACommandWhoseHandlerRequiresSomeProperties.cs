using System;
using System.Linq;
using System.Threading.Tasks;
using Nimbus.Configuration;
using Nimbus.IntegrationTests.Tests.SimpleCommandSendingTests.CommandHandlers;
using Nimbus.IntegrationTests.Tests.SimpleCommandSendingTests.MessageContracts;
using Nimbus.Tests.Common;
using Nimbus.Tests.Common.Extensions;
using Nimbus.Tests.Common.TestScenarioGeneration;
using Nimbus.Tests.Common.TestScenarioGeneration.ConfigurationSources;
using Nimbus.Tests.Common.TestScenarioGeneration.ScenarioComposition;
using Nimbus.Tests.Common.TestScenarioGeneration.TestCaseSources;
using Nimbus.Tests.Common.TestUtilities;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.IntegrationTests.Tests.SimpleCommandSendingTests
{
    [TestFixture]
    public class WhenSendingACommandWhoseHandlerRequiresSomeProperties : TestForBus
    {
        protected override async Task When()
        {
            SomeOtherCommandHandler.Clear();
            var someCommand = new SomeOtherCommand();
            await Bus.Send(someCommand);
            await TimeSpan.FromSeconds(TimeoutSeconds).WaitUntil(() => MethodCallCounter.AllReceivedMessages.Any());
        }

        [Test]
        [TestCaseSource(typeof (AllBusConfigurations<WhenSendingACommandWhoseHandlerRequiresSomeProperties>))]
        public async Task TheCommandBrokerShouldReceiveThatCommand(string testName, IConfigurationScenario<BusBuilderConfiguration> scenario)
        {
            await Given(scenario);
            await When();

            MethodCallCounter.AllReceivedMessages.OfType<SomeOtherCommand>().Count().ShouldBe(1);
        }

        [Test]
        [TestCaseSource(typeof (AllBusConfigurations<WhenSendingACommandWhoseHandlerRequiresSomeProperties>))]
        public async Task TheDispatchContextShouldBeSet(string testName, IConfigurationScenario<BusBuilderConfiguration> scenario)
        {
            await Given(scenario);
            await When();

            SomeOtherCommandHandler.ReceivedDispatchContext.ShouldNotBe(null);
        }

        [Test]
        [TestCaseSource(typeof (AllBusConfigurations<WhenSendingACommandWhoseHandlerRequiresSomeProperties>))]
        public async Task TheMessagePropertiesShouldBeSet(string testName, IConfigurationScenario<BusBuilderConfiguration> scenario)
        {
            await Given(scenario);
            await When();

            SomeOtherCommandHandler.ReceivedMessageProperties.ShouldNotBe(null);
        }

        [Test]
        [TestCaseSource(typeof (AllBusConfigurations<WhenSendingACommandWhoseHandlerRequiresSomeProperties>))]
        public async Task TheCorrectNumberOfTotalMessagesShouldHaveBeenObserved(string testName, IConfigurationScenario<BusBuilderConfiguration> scenario)
        {
            await Given(scenario);
            await When();

            MethodCallCounter.AllReceivedMessages.Count().ShouldBe(1);
        }
    }
}