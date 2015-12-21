using System;
using System.Threading.Tasks;
using Nimbus.Configuration;
using Nimbus.IntegrationTests.Tests.SimpleRequestResponseTests.MessageContracts;
using Nimbus.Tests.Common.TestScenarioGeneration;
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
        public async Task WeShouldGetSomethingNiceBack(string testName, BusBuilderConfiguration busBuilderConfiguration)
        {
            await Given(busBuilderConfiguration);
            await When();

            _response.ShouldNotBe(null);
        }
    }
}