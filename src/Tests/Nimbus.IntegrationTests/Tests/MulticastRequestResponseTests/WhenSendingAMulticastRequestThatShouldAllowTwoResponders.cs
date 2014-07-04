using System;
using System.Linq;
using System.Threading.Tasks;
using Nimbus.IntegrationTests.Tests.MulticastRequestResponseTests.MessageContracts;
using Nimbus.Tests.Common;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.IntegrationTests.Tests.MulticastRequestResponseTests
{
    [TestFixture]
    public class WhenSendingAMulticastRequestThatShouldAllowTwoResponders : TestForBus
    {
        private BlackBallResponse[] _response;

        protected override async Task When()
        {
            var request = new BlackBallRequest
                          {
                              ProspectiveMemberName = "Fred Flintstone",
                          };

            _response = (await Bus.MulticastRequest(request, TimeSpan.FromSeconds(3))).ToArray();
        }

        [Test]
        public async Task WeShouldReceiveTwoResponses()
        {
            _response.Count().ShouldBe(2);
        }

        [Test]
        public async Task AllHandlersShouldHaveAtLeastReceivedTheRequest()
        {
            MethodCallCounter.AllReceivedMessages.OfType<BlackBallRequest>().Count().ShouldBe(4);
        }
    }
}