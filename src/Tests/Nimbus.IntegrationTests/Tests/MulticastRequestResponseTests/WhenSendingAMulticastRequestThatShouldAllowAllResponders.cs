using System;
using System.Linq;
using System.Threading.Tasks;
using Nimbus.Configuration;
using Nimbus.IntegrationTests.Tests.MulticastRequestResponseTests.MessageContracts;
using Nimbus.IntegrationTests.Tests.MulticastRequestResponseTests.RequestHandlers;
using Nimbus.Tests.Common.TestScenarioGeneration.ConfigurationSources;
using Nimbus.Tests.Common.TestScenarioGeneration.TestCaseSources;
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
            SlowBlackBallRequestHandler.Reset();

            var request = new BlackBallRequest
                          {
                              ProspectiveMemberName = "Fred Flintstone"
                          };

            var requestTask = Bus.MulticastRequest(request, TimeSpan.FromSeconds(TimeoutSeconds));
            SlowBlackBallRequestHandler.HandlerThrottle.Release(1);
            _response = (await requestTask)
                .Take(3)
                .ToArray();
        }

        [Test]
        [TestCaseSource(typeof (AllBusConfigurations<WhenSendingAMulticastRequestThatShouldAllowAllResponders>))]
        public async Task WeShouldReceiveThreeResponses(string testName, IConfigurationScenario<BusBuilderConfiguration> scenario)
        {
            await Given(scenario);
            await When();

            _response.Count().ShouldBe(3);
        }

        [Test]
        [TestCaseSource(typeof (AllBusConfigurations<WhenSendingAMulticastRequestThatShouldAllowAllResponders>))]
        public async Task AllHandlersShouldHaveAtLeastReceivedTheRequest(string testName, IConfigurationScenario<BusBuilderConfiguration> scenario)
        {
            await Given(scenario);
            await When();

            MethodCallCounter.AllReceivedMessages.OfType<BlackBallRequest>().Count().ShouldBe(4);
        }
    }
}