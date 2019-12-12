using System.Linq;
using System.Threading.Tasks;
using Nimbus.Configuration;
using Nimbus.Tests.Common.Extensions;
using Nimbus.Tests.Integration.Tests.SimpleCommandSendingTests.CommandHandlers;
using Nimbus.Tests.Integration.Tests.SimpleCommandSendingTests.MessageContracts;
using Nimbus.Tests.Integration.TestScenarioGeneration.ScenarioComposition;
using Nimbus.Tests.Integration.TestScenarioGeneration.TestCaseSources;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.Tests.Integration.Tests.SimpleCommandSendingTests
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