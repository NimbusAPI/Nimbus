using System;
using System.Threading.Tasks;
using Nimbus.Exceptions;
using Nimbus.IntegrationTests.InfrastructureContracts;
using Nimbus.IntegrationTests.Tests.ExceptionPropagationTests.MessageContracts;
using Nimbus.IntegrationTests.Tests.ExceptionPropagationTests.RequestHandlers;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.IntegrationTests.Tests.ExceptionPropagationTests
{
    public class WhenSendingARequestThatWillThrow : TestForAllBuses
    {
        private RequestThatWillThrowResponse _response;
        private Exception _exception;

        public override Task Given(ITestHarnessBusFactory busFactory)
        {
            _response = null;
            _exception = null;

            return base.Given(busFactory);
        }

        public override async Task When()
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
        [TestCaseSource("AllBusesTestCases")]
        public async void TheResponseShouldNotBeSet(ITestHarnessBusFactory busFactory)
        {
            await Given(busFactory);
            await When();

            _response.ShouldBe(null);
        }

        [Test]
        [TestCaseSource("AllBusesTestCases")]
        public async void AnExceptionShouldBeReThrownOnTheClient(ITestHarnessBusFactory busFactory)
        {
            await Given(busFactory);
            await When();

            _exception.ShouldNotBe(null);
        }

        [Test]
        [TestCaseSource("AllBusesTestCases")]
        public async void TheExceptionShouldBeARequestFailedException(ITestHarnessBusFactory busFactory)
        {
            await Given(busFactory);
            await When();

            _exception.ShouldBeTypeOf<RequestFailedException>();
        }

        [Test]
        [TestCaseSource("AllBusesTestCases")]
        public async void TheExceptionShouldContainTheMessageThatWasThrownOnTheServer(ITestHarnessBusFactory busFactory)
        {
            await Given(busFactory);
            await When();

            _exception.Message.ShouldContain(RequestThatWillThrowHandler.ExceptionMessage);
        }
    }
}