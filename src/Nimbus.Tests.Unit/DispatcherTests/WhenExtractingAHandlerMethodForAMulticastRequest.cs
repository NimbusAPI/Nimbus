using Nimbus.Infrastructure.RequestResponse;
using Nimbus.MessageContracts;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.Tests.Unit.DispatcherTests
{
    public class WhenExtractingAHandlerMethodForAMulticastRequest
    {
        [Test]
        public void WeShouldGetTheRightMethod()
        {
            var request = new SomeInternalMulticastRequest();
            var handlerMethod = MulticastRequestMessageDispatcher.GetGenericDispatchMethodFor(request);

            handlerMethod.ShouldNotBe(null);
        }

        public class SomeInternalMulticastRequest : BusMulticastRequest<SomeInternalMulticastRequest, SomeInternalMulticastResponse>
        {
        }

        public class SomeInternalMulticastResponse : IBusMulticastResponse
        {
        }
    }
}