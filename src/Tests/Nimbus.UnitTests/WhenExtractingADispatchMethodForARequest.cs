using Nimbus.Infrastructure.RequestResponse;
using Nimbus.MessageContracts;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.UnitTests
{
    public class WhenExtractingADispatchMethodForARequest
    {
        [Test]
        public void WeShouldGetTheRightMethod()
        {
            var request = new SomeInternalRequest();
            var handlerMethod = RequestMessageDispatcher.GetGenericDispatchMethodFor(request);

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