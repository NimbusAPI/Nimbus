using System;
using System.Threading.Tasks;
using Nimbus.Configuration;
using Nimbus.Exceptions;
using Nimbus.IntegrationTests.Tests.ExceptionPropagationTests.MessageContracts;
using Nimbus.IntegrationTests.Tests.ExceptionPropagationTests.RequestHandlers;
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
            using (new SingleThreadedSynchronizationContext())
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
        }

        [Test]
        [TestCaseSource(typeof (AllBusConfigurations<WhenSendingARequestThatWillThrow>))]
        public async Task Run(string testName, IConfigurationScenario<BusBuilderConfiguration> scenario)
        {
            await Given(scenario);
            await When();
            await Then();
        }

        [Then]
        public async Task TheResponseShouldNotBeSet()
        {
            _response.ShouldBe(null);
        }

        [Then]
        public async Task AnExceptionShouldBeReThrownOnTheClient()
        {
            _exception.ShouldNotBe(null);
        }

        [Then]
        public async Task TheExceptionShouldBeARequestFailedException()
        {
            _exception.ShouldBeTypeOf<RequestFailedException>();
        }

        [Then]
        public async Task TheExceptionShouldContainTheMessageThatWasThrownOnTheServer()
        {
            _exception.Message.ShouldContain(RequestThatWillThrowHandler.ExceptionMessage);
        }
    }
}