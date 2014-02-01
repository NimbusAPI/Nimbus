using System;
using System.Threading.Tasks;
using Nimbus.IntegrationTests.InfrastructureContracts;
using Nimbus.IntegrationTests.Tests.SimpleRequestResponseTests.MessageContracts;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.IntegrationTests.Tests.SimpleRequestResponseTests
{
    [TestFixture]
    public class WhenSendingARequestOnTheBus : TestForAllBuses
    {
        private SomeResponse _response;

        private readonly TimeSpan _timeout = TimeSpan.FromSeconds(10);

        public override async Task When()
        {
            _response = await Bus.Request(new SomeRequest(), _timeout);
        }

        [Test]
        [TestCaseSource("AllBusesTestCases")]
        public async void WeShouldGetSomethingNiceBack(ITestHarnessBusFactory busFactory)
        {
            await Given(busFactory);
            await When();

            _response.ShouldNotBe(null);
        }
    }
}