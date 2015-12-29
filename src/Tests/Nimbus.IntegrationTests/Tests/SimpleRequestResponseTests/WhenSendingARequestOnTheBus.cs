using System;
using System.Threading.Tasks;
using Nimbus.Configuration;
using Nimbus.IntegrationTests.Tests.SimpleRequestResponseTests.MessageContracts;
using Nimbus.Tests.Common.TestScenarioGeneration;
using Nimbus.Tests.Common.TestScenarioGeneration.ConfigurationSources;
using Nimbus.Tests.Common.TestScenarioGeneration.TestCaseSources;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.IntegrationTests.Tests.SimpleRequestResponseTests
{
    [TestFixture]
    public class WhenSendingARequestOnTheBus : TestForBus
    {
        private SomeResponse _response;

        private readonly TimeSpan _timeout = TimeSpan.FromSeconds(10);

        protected override async Task When()
        {
            _response = await Bus.Request(new SomeRequest(), _timeout);
        }

        [Test]
        [TestCaseSource(typeof (AllBusConfigurations<WhenSendingARequestOnTheBus>))]
        public async Task WeShouldGetSomethingNiceBack(string testName, IConfigurationScenario<BusBuilderConfiguration> scenario)
        {
            await Given(scenario);
            await When();

            _response.ShouldNotBe(null);
        }
    }
}