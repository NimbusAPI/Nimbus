using System;
using System.Linq;
using System.Threading.Tasks;
using Nimbus.Configuration;
using Nimbus.IntegrationTests.Tests.MulticastRequestResponseTests.MessageContracts;
using Nimbus.Tests.Common;
using Nimbus.Tests.Common.TestScenarioGeneration;
using Nimbus.Tests.Common.TestUtilities;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.IntegrationTests.Tests.MulticastRequestResponseTests
{
    [TestFixture]
    public class WhenSendingAMulticastRequestThatShouldAllowAllResponders : TestForBus
    {
        private BlackBallResponse[] _response;

        protected override async Task When()
        {
            var request = new BlackBallRequest
                          {
                              ProspectiveMemberName = "Fred Flintstone"
                          };

            _response = (await Bus.MulticastRequest(request, TimeSpan.FromSeconds(6))).ToArray();
        }

        [Test]
        [TestCaseSource(typeof (AllBusConfigurations<WhenSendingAMulticastRequestThatShouldAllowAllResponders>))]
        public async Task WeShouldReceiveThreeResponses(string testName, BusBuilderConfiguration busBuilderConfiguration)
        {
            await Given(busBuilderConfiguration);
            await When();

            _response.Count().ShouldBe(3);
        }

        [Test]
        [TestCaseSource(typeof (AllBusConfigurations<WhenSendingAMulticastRequestThatShouldAllowAllResponders>))]
        public async Task AllHandlersShouldHaveAtLeastReceivedTheRequest(string testName, BusBuilderConfiguration busBuilderConfiguration)
        {
            await Given(busBuilderConfiguration);
            await When();

            MethodCallCounter.AllReceivedMessages.OfType<BlackBallRequest>().Count().ShouldBe(4);
        }
    }
}