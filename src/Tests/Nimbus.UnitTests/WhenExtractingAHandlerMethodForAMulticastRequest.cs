using System.Reflection;
using Nimbus.MessageContracts;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.UnitTests
{
    public class WhenExtractingAHandlerMethodForAMulticastRequest
    {
        [Test]
        public void WeShouldGetTheRightMethod()
        {
            var request = new SomeInternalRequest();
            var handlerMethod = MulticastRequestMessagePump.ExtractHandleMulticastMethodInfo(request);

            ShouldBeTestExtensions.ShouldNotBe<MethodInfo>(handlerMethod, null);
        }

        public class SomeInternalRequest : BusRequest<SomeInternalRequest, SomeInternalResponse>
        {
        }

        public class SomeInternalResponse : IBusResponse
        {
        }
    }
}