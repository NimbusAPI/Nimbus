using System;
using System.Threading.Tasks;
using Nimbus.Exceptions;
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

        public override async Task When(ITestHarnessBusFactory busFactory)
        {
            _response = null;
            _exception = null;

            var bus = busFactory.Create();

            try
            {
                var request = new RequestThatWillThrow();
                _response = await bus.Request(request);
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
            await When(busFactory);

            _response.ShouldBe(null);
        }

        [Test]
        [TestCaseSource("AllBusesTestCases")]
        public async void AnExceptionShouldBeReThrownOnTheClient(ITestHarnessBusFactory busFactory)
        {
            await When(busFactory);

            _exception.ShouldNotBe(null);
        }

        [Test]
        [TestCaseSource("AllBusesTestCases")]
        public async void TheExceptionShouldBeARequestFailedException(ITestHarnessBusFactory busFactory)
        {
            await When(busFactory);

            _exception.ShouldBeTypeOf<RequestFailedException>();
        }

        [Test]
        [TestCaseSource("AllBusesTestCases")]
        public async void TheExceptionShouldContainTheMessageThatWasThrownOnTheServer(ITestHarnessBusFactory busFactory)
        {
            await When(busFactory);

            _exception.Message.ShouldContain(RequestThatWillThrowHandler.ExceptionMessage);
        }
    }
}