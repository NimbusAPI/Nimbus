using System.Threading.Tasks;
using Nimbus.Configuration;
using Nimbus.IntegrationTests.Tests.SimpleRequestResponseTests.MessageContracts;
using Nimbus.IntegrationTests.TestScenarioGeneration.TestCaseSources;
using Nimbus.Tests.Common.TestScenarioGeneration.ScenarioComposition;
using Nimbus.Tests.Common.TestScenarioGeneration.TestCaseSources;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.IntegrationTests.Tests.SimpleRequestResponseTests
{
    [TestFixture]
    public class WhenSendingARequestOnTheBus : TestForBus
    {
        private SomeResponse _response;

        protected override async Task When()
        {
            _response = await Bus.Request(new SomeRequest(), Timeout);
        }

        [Test]
        [TestCaseSource(typeof (AllBusConfigurations<WhenSendingARequestOnTheBus>))]
        public async Task Run(string testName, IConfigurationScenario<BusBuilderConfiguration> scenario)
        {
            await Given(scenario);
            await When();
            await Then();
        }

        [Then]
        public async Task WeShouldGetSomethingNiceBack()
        {
            _response.ShouldBeOfType<SomeResponse>();
        }
    }
}