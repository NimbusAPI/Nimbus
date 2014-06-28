using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nimbus.IntegrationTests.Tests.MulticastRequestResponseTests.MessageContracts;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.IntegrationTests.Tests.MulticastRequestResponseTests
{
    [TestFixture]
    public class WhenSendingAMulticastRequestThatShouldAllowTwoResponders : TestForBus
    {
        private IEnumerable<BlackBallResponse> _response;

        protected override async Task When()
        {
            var request = new BlackBallRequest
                          {
                              ProspectiveMemberName = "Fred Flintstone",
                          };

            _response = await Bus.MulticastRequest(request, TimeSpan.FromSeconds(3));
        }

        [Test]
        public async Task WeShouldReceiveTwoResponses()
        {
            _response.Count().ShouldBe(2);
        }
    }
}