using System;
using System.Linq;
using System.Threading.Tasks;
using Nimbus.Configuration;
using Nimbus.Tests.Integration.Tests.AbstractBaseTypeMessageTests.MessageContracts;
using Nimbus.Tests.Integration.TestScenarioGeneration.ScenarioComposition;
using Nimbus.Tests.Integration.TestScenarioGeneration.ScenarioComposition.Filters;
using Nimbus.Tests.Integration.TestScenarioGeneration.TestCaseSources;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.Tests.Integration.Tests.AbstractBaseTypeMessageTests
{
    [TestFixture]
    [FilterTestCasesBy(typeof(InProcessScenariosFilter))]
    public class WhenSendingARequestThatHasAnAbstractBaseType : TestForBus
    {
        private SomeConcreteResponseType _response;

        protected override async Task When()
        {
            var request = new SomeConcreteRequestType();
            _response = await Bus.Request(request, TimeSpan.FromSeconds(TimeoutSeconds));
        }

        [Test]
        [TestCaseSource(typeof (AllBusConfigurations<WhenSendingARequestThatHasAnAbstractBaseType>))]
        public async Task Run(string testName, IConfigurationScenario<BusBuilderConfiguration> scenario)
        {
            await Given(scenario);
            await When();
            await Then();
        }

        [Then]
        public async Task TheHandlerShouldReceiveThatRequest()
        {
            MethodCallCounter.AllReceivedMessages.OfType<SomeConcreteRequestType>().Count().ShouldBe(1);
        }

        [Then]
        public async Task TheCorrectNumberOfTotalMessagesShouldHaveBeenObserved()
        {
            MethodCallCounter.AllReceivedMessages.Count().ShouldBe(1);
        }

        [Then]
        public async Task TheResponseShouldNotBeNull()
        {
            _response.ShouldNotBe(null);
        }
    }
}