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

        public override Task Given()
        {
            _response = null;
            _exception = null;

            return base.Given();
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
        public async Task TheResponseShouldNotBeSet()
        {
            await Given();
            await When();

            _response.ShouldBe(null);
        }

        [Test]
        public async Task AnExceptionShouldBeReThrownOnTheClient()
        {
            await Given();
            await When();

            _exception.ShouldNotBe(null);
        }

        [Test]
        public async Task TheExceptionShouldBeARequestFailedException()
        {
            await Given();
            await When();

            _exception.ShouldBeTypeOf<RequestFailedException>();
        }

        [Test]
        public async Task TheExceptionShouldContainTheMessageThatWasThrownOnTheServer()
        {
            await Given();
            await When();

            _exception.Message.ShouldContain(RequestThatWillThrowHandler.ExceptionMessage);
        }
    }
}