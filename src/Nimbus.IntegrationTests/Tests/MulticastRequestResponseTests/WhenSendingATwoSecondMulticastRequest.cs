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
    public class WhenSendingATwoSecondMulticastRequest : SpecificationForBus
    {
        private IEnumerable<BlackBallResponse> _response;

        public override async Task WhenAsync()
        {
            var request = new BlackBallRequest
                          {
                              ProspectiveMemberName = "Fred Flintstone",
                          };

            _response = await Subject.MulticastRequest(request, TimeSpan.FromSeconds(2));
        }

        [Test]
        public void WeShouldReceiveTwoResponses()
        {
            _response.Count().ShouldBe(2);
        }
    }
}