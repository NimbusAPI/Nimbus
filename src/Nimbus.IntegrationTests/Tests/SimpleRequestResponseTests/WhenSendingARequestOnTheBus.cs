using System;
using System.Threading.Tasks;
using Nimbus.IntegrationTests.Tests.SimpleRequestResponseTests.MessageContracts;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.IntegrationTests.Tests.SimpleRequestResponseTests
{
    [TestFixture]
    public class WhenSendingARequestOnTheBus : SpecificationForBus
    {
        private SomeResponse _response;

        private readonly TimeSpan _timeout = TimeSpan.FromSeconds(10);

        public override async Task WhenAsync()
        {
            _response = await Subject.Request(new SomeRequest(), _timeout);
        }

        [Test]
        public void WeShouldGetSomethingNiceBack()
        {
            _response.ShouldNotBe(null);
        }
    }
}