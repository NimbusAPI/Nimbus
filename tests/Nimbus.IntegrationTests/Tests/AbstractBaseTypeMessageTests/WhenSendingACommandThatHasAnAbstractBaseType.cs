using System.Linq;
using System.Threading.Tasks;
using Nimbus.Configuration;
using Nimbus.IntegrationTests.Tests.AbstractBaseTypeMessageTests.MessageContracts;
using Nimbus.IntegrationTests.TestScenarioGeneration.TestCaseSources;
using Nimbus.Tests.Common.Extensions;
using Nimbus.Tests.Common.TestScenarioGeneration.ScenarioComposition;
using Nimbus.Tests.Common.TestScenarioGeneration.ScenarioComposition.Filters;
using Nimbus.Tests.Common.TestScenarioGeneration.TestCaseSources;
using Nimbus.Tests.Common.TestUtilities;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.IntegrationTests.Tests.AbstractBaseTypeMessageTests
{
    [TestFixture]
    [FilterTestCasesBy(typeof(InProcessScenariosFilter))]
    public class WhenSendingACommandThatHasAnAbstractBaseType : TestForBus
    {
        protected override async Task When()
        {
            var someCommand = new SomeConcreteCommandType();
            await Bus.Send(someCommand);
            await Timeout.WaitUntil(() => MethodCallCounter.AllReceivedMessages.Any());
        }

        [Test]
        [TestCaseSource(typeof (AllBusConfigurations<WhenSendingACommandThatHasAnAbstractBaseType>))]
        public async Task Run(string testName, IConfigurationScenario<BusBuilderConfiguration> scenario)
        {
            await Given(scenario);
            await When();
            await Then();
        }

        [Then]
        public async Task TheCommandBrokerShouldReceiveThatCommand()
        {
            MethodCallCounter.AllReceivedMessages.OfType<SomeConcreteCommandType>().Count().ShouldBe(1);
        }

        [Then]
        public async Task TheCorrectNumberOfTotalMessagesShouldHaveBeenObserved()
        {
            MethodCallCounter.AllReceivedMessages.Count().ShouldBe(1);
        }
    }
}