using NUnit.Framework;
using Shouldly;

namespace Nimbus.Tests
{
    public class WhenExtractingAHandlerMethodForAMessage
    {
        [Test]
        public void WeShouldGetTheRightMethod()
        {
            var request = new SomeInternalRequest();
            var handlerMethod = RequestBrokerHandlerExtensions.ExtractHandlerMethodInfo(request);

            handlerMethod.ShouldNotBe(null);
        }

        public class SomeInternalRequest : BusRequest<SomeInternalRequest, SomeInternalResponse>
        {
        }

        public class SomeInternalResponse
        {
        }
    }
}