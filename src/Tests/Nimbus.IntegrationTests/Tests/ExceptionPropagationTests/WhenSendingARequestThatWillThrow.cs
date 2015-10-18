using System;
using System.Threading.Tasks;
using Nimbus.Exceptions;
using Nimbus.IntegrationTests.Tests.ExceptionPropagationTests.MessageContracts;
using Nimbus.IntegrationTests.Tests.ExceptionPropagationTests.RequestHandlers;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.IntegrationTests.Tests.ExceptionPropagationTests
{
    [TestFixture]
    public class WhenSendingARequestThatWillThrow : TestForBus
    {
        private RequestThatWillThrowResponse _response;
        private Exception _exception;

        protected override Task Given()
        {
            _response = null;
            _exception = null;

            return base.Given();
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
        public async Task TheResponseShouldNotBeSet()
        {
            _response.ShouldBe(null);
        }

        [Test]
        public async Task AnExceptionShouldBeReThrownOnTheClient()
        {
            _exception.ShouldNotBe(null);
        }

        [Test]
        public async Task TheExceptionShouldBeARequestFailedException()
        {
            _exception.ShouldBeOfType<RequestFailedException>();
        }

        [Test]
        public async Task TheExceptionShouldContainTheMessageThatWasThrownOnTheServer()
        {
            _exception.Message.ShouldContain(RequestThatWillThrowHandler.ExceptionMessage);
        }
    }
}