using System;
using System.Linq;
using System.Threading.Tasks;
using Nimbus.Configuration;
using Nimbus.IntegrationTests.Tests.AbstractBaseTypeMessageTests.MessageContracts;
using Nimbus.Tests.Common.TestScenarioGeneration.ScenarioComposition;
using Nimbus.Tests.Common.TestScenarioGeneration.TestCaseSources;
using Nimbus.Tests.Common.TestUtilities;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.IntegrationTests.Tests.AbstractBaseTypeMessageTests
{
    public class WhenSendingAMulticastRequestThatHasAnAbstractBaseType : TestForBus
    {
        private SomeConcreteResponseType[] _response;

        protected override async Task When()
        {
            var request = new SomeConcreteRequestType();
            _response = (await Bus.MulticastRequest(request, TimeSpan.FromSeconds(TimeoutSeconds)))
                .Take(1)
                .ToArray();
        }

        [Test]
        [TestCaseSource(typeof (AllBusConfigurations<WhenSendingAMulticastRequestThatHasAnAbstractBaseType>))]
        public async Task Run(string testName, IConfigurationScenario<BusBuilderConfiguration> scenario)
        {
            await Given(scenario);
            await When();

            _response.ShouldNotBe(null);
        }

        [Then]
        public async Task TheResponseShouldNotBeNull()
        {
            _response.ShouldNotBe(null);
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
    }
}