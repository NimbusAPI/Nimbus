using System;
using System.Threading.Tasks;
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

        public override async Task When(ITestHarnessBusFactory busFactory)
        {
            var bus = busFactory.Create();

            _response = await bus.Request(new SomeRequest(), _timeout);
        }

        [Test]
        [TestCaseSource("AllBusesTestCases")]
        public async void WeShouldGetSomethingNiceBack(ITestHarnessBusFactory busFactory)
        {
            await When(busFactory);

            _response.ShouldNotBe(null);
        }
    }
}