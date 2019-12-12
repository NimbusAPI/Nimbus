using System.Linq;
using System.Threading.Tasks;
using Nimbus.Configuration;
using Nimbus.IntegrationTests.Tests.SimpleCommandSendingTests.CommandHandlers;
using Nimbus.IntegrationTests.Tests.SimpleCommandSendingTests.MessageContracts;
using Nimbus.IntegrationTests.TestScenarioGeneration.TestCaseSources;
using Nimbus.Tests.Common.Extensions;
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
            await Timeout.WaitUntil(() => MethodCallCounter.AllReceivedMessages.Any());
        }

        [Test]
        [TestCaseSource(typeof (AllBusConfigurations<WhenSendingACommandWhoseHandlerRequiresSomeProperties>))]
        public async Task Run(string testName, IConfigurationScenario<BusBuilderConfiguration> scenario)
        {
            await Given(scenario);
            await When();
            await Then();
        }

        [Then]
        public async Task TheCommandBrokerShouldReceiveThatCommand()
        {
            MethodCallCounter.AllReceivedMessages.OfType<SomeOtherCommand>().Count().ShouldBe(1);
        }

        [Then]
        public async Task TheDispatchContextShouldBeSet()
        {
            SomeOtherCommandHandler.ReceivedDispatchContext.ShouldNotBe(null);
        }

        [Then]
        public async Task TheMessagePropertiesShouldBeSet()
        {
            SomeOtherCommandHandler.ReceivedMessageProperties.ShouldNotBe(null);
        }

        [Then]
        public async Task TheCorrectNumberOfTotalMessagesShouldHaveBeenObserved()
        {
            MethodCallCounter.AllReceivedMessages.Count().ShouldBe(1);
        }
    }
}