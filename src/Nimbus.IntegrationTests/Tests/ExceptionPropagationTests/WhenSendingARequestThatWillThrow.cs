using System;
using System.Threading.Tasks;
using Nimbus.Exceptions;
using Nimbus.IntegrationTests.Tests.ExceptionPropagationTests.MessageContracts;
using Nimbus.IntegrationTests.Tests.ExceptionPropagationTests.RequestHandlers;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.IntegrationTests.Tests.ExceptionPropagationTests
{
    public class WhenSendingARequestThatWillThrow : SpecificationForBus
    {
        private RequestThatWillThrowResponse _response;
        private Exception _exception;

        public override async Task WhenAsync()
        {
            try
            {
                var request = new RequestThatWillThrow();
                _response = await Subject.Request(request);
            }
            catch (RequestFailedException exc)
            {
                _exception = exc;
            }
        }

        [Test]
        public void TheResponseShouldNotBeSet()
        {
            _response.ShouldBe(null);
        }

        [Test]
        public void AnExceptionShouldBeReThrownOnTheClient()
        {
            _exception.ShouldNotBe(null);
        }

        [Test]
        public void TheExceptionShouldBeARequestFailedException()
        {
            _exception.ShouldBeTypeOf<RequestFailedException>();
        }

        [Test]
        public void TheExceptionShouldHaveTheMessageThatWasThrownOnTheServer()
        {
            _exception.Message.ShouldBe(RequestThatWillThrowHandler.ExceptionMessage);
        }
    }
}