using System;
using System.Threading.Tasks;
using Nimbus.Configuration;
using Nimbus.Exceptions;
using Nimbus.IntegrationTests.Tests.ExceptionPropagationTests.MessageContracts;
using Nimbus.IntegrationTests.Tests.ExceptionPropagationTests.RequestHandlers;
using Nimbus.Tests.Common.TestScenarioGeneration.ConfigurationSources;
using Nimbus.Tests.Common.TestScenarioGeneration.ScenarioComposition;
using Nimbus.Tests.Common.TestScenarioGeneration.TestCaseSources;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.IntegrationTests.Tests.ExceptionPropagationTests
{
    [TestFixture]
    public class WhenSendingARequestThatWillThrow : TestForBus
    {
        private RequestThatWillThrowResponse _response;
        private Exception _exception;

        protected override Task Given(IConfigurationScenario<BusBuilderConfiguration> scenario)
        {
            _response = null;
            _exception = null;

            return base.Given(scenario);
        }

        protected override async Task When()
        {
            try
            {
                var request = new RequestThatWillThrow();
                _response = await Bus.Request(request);
            }
            catch (RequestFailedException exc)
            {
                _exception = exc;
            }
        }

        [Test]
        [TestCaseSource(typeof (AllBusConfigurations<WhenSendingARequestThatWillThrow>))]
        public async Task TheResponseShouldNotBeSet(string testName, IConfigurationScenario<BusBuilderConfiguration> scenario)
        {
            await Given(scenario);
            await When();

            _response.ShouldBe(null);
        }

        [Test]
        [TestCaseSource(typeof (AllBusConfigurations<WhenSendingARequestThatWillThrow>))]
        public async Task AnExceptionShouldBeReThrownOnTheClient(string testName, IConfigurationScenario<BusBuilderConfiguration> scenario)
        {
            await Given(scenario);
            await When();

            _exception.ShouldNotBe(null);
        }

        [Test]
        [TestCaseSource(typeof (AllBusConfigurations<WhenSendingARequestThatWillThrow>))]
        public async Task TheExceptionShouldBeARequestFailedException(string testName, IConfigurationScenario<BusBuilderConfiguration> scenario)
        {
            await Given(scenario);
            await When();

            _exception.ShouldBeTypeOf<RequestFailedException>();
        }

        [Test]
        [TestCaseSource(typeof (AllBusConfigurations<WhenSendingARequestThatWillThrow>))]
        public async Task TheExceptionShouldContainTheMessageThatWasThrownOnTheServer(string testName, IConfigurationScenario<BusBuilderConfiguration> scenario)
        {
            await Given(scenario);
            await When();

            _exception.Message.ShouldContain(RequestThatWillThrowHandler.ExceptionMessage);
        }
    }
}