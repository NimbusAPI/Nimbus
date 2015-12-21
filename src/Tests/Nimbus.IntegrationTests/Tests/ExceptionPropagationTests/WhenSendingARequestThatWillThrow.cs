using System;
using System.Threading.Tasks;
using Nimbus.Configuration;
using Nimbus.Exceptions;
using Nimbus.IntegrationTests.Tests.ExceptionPropagationTests.MessageContracts;
using Nimbus.IntegrationTests.Tests.ExceptionPropagationTests.RequestHandlers;
using Nimbus.Tests.Common.TestScenarioGeneration;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.IntegrationTests.Tests.ExceptionPropagationTests
{
    [TestFixture]
    public class WhenSendingARequestThatWillThrow : TestForBus
    {
        private RequestThatWillThrowResponse _response;
        private Exception _exception;

        protected override Task Given(BusBuilderConfiguration busBuilderConfiguration)
        {
            _response = null;
            _exception = null;

            return base.Given(busBuilderConfiguration);
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
        public async Task TheResponseShouldNotBeSet(string testName, BusBuilderConfiguration busBuilderConfiguration)
        {
            await Given(busBuilderConfiguration);
            await When();

            _response.ShouldBe(null);
        }

        [Test]
        [TestCaseSource(typeof (AllBusConfigurations<WhenSendingARequestThatWillThrow>))]
        public async Task AnExceptionShouldBeReThrownOnTheClient(string testName, BusBuilderConfiguration busBuilderConfiguration)
        {
            await Given(busBuilderConfiguration);
            await When();

            _exception.ShouldNotBe(null);
        }

        [Test]
        [TestCaseSource(typeof (AllBusConfigurations<WhenSendingARequestThatWillThrow>))]
        public async Task TheExceptionShouldBeARequestFailedException(string testName, BusBuilderConfiguration busBuilderConfiguration)
        {
            await Given(busBuilderConfiguration);
            await When();

            _exception.ShouldBeTypeOf<RequestFailedException>();
        }

        [Test]
        [TestCaseSource(typeof (AllBusConfigurations<WhenSendingARequestThatWillThrow>))]
        public async Task TheExceptionShouldContainTheMessageThatWasThrownOnTheServer(string testName, BusBuilderConfiguration busBuilderConfiguration)
        {
            await Given(busBuilderConfiguration);
            await When();

            _exception.Message.ShouldContain(RequestThatWillThrowHandler.ExceptionMessage);
        }
    }
}