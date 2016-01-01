using System;
using System.Linq;
using System.Threading.Tasks;
using Nimbus.Configuration;
using Nimbus.IntegrationTests.Tests.LargeMessageTests.Handlers;
using Nimbus.IntegrationTests.Tests.LargeMessageTests.MessageContracts;
using Nimbus.Tests.Common.TestScenarioGeneration.ScenarioComposition;
using Nimbus.Tests.Common.TestScenarioGeneration.ScenarioComposition.Filters;
using Nimbus.Tests.Common.TestScenarioGeneration.TestCaseSources;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.IntegrationTests.Tests.LargeMessageTests
{
    [TestFixture]
    [FilterTestCasesBy(typeof (OnlyLargeMessagesFilter))]
    public class WhenSendingALargeRequest : TestForBus
    {
        private BigFatResponse _response;
        private BigFatRequest _busRequest;

        protected override async Task When()
        {
            var bigQuestion = new string(Enumerable.Range(0, BigFatRequestHandler.MessageSize).Select(i => '.').ToArray());

            _busRequest = new BigFatRequest
                          {
                              SomeBigQuestion = bigQuestion
                          };
            _response = await Bus.Request(_busRequest, TimeSpan.FromSeconds(TimeoutSeconds));
        }

        [Test]
        [TestCaseSource(typeof (AllBusConfigurations<WhenSendingALargeRequest>))]
        public async Task TheResponseShouldReturnUnscathed(string testName, IConfigurationScenario<BusBuilderConfiguration> scenario)
        {
            await Given(scenario);
            await When();

            _response.SomeBigAnswer.Length.ShouldBe(BigFatRequestHandler.MessageSize);
        }
    }
}