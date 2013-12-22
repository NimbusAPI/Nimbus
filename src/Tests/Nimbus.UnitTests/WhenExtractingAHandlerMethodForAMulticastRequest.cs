using NUnit.Framework;
using Nimbus.Infrastructure.RequestResponse;
using Nimbus.MessageContracts;
using Shouldly;

namespace Nimbus.Tests
{
    public class WhenExtractingAHandlerMethodForAMulticastRequest
    {
        [Test]
        public void WeShouldGetTheRightMethod()
        {
            var request = new SomeInternalRequest();
            var handlerMethod = MulticastRequestMessagePump.ExtractHandleMulticastMethodInfo(request);

            handlerMethod.ShouldNotBe(null);
        }

        public class SomeInternalRequest : BusRequest<SomeInternalRequest, SomeInternalResponse>
        {
        }

        public class SomeInternalResponse : IBusResponse
        {
        }
    }
}