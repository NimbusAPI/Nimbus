using System;
using Nimbus.DependencyResolution;
using Nimbus.Interceptors.Inbound;
using Nimbus.UnitTests.DispatcherTests.Handlers;
using Nimbus.UnitTests.DispatcherTests.MessageContracts;
using NSubstitute;
using NUnit.Framework;

namespace Nimbus.UnitTests.DispatcherTests
{
    public class WhenHandlerThrowsAnExceptionInRequestMessageDispatcher
        : MessageDispatcherTestBase
    {
        [Test]
        public void TheExceptionIsBubbledThroughTheInterceptors()
        {
            var interceptor = Substitute.For<IInboundInterceptor>();
            var dispatcher = GetRequestMessageDispatcher<ExceptingRequest, ExceptingResponse, ExceptingRequestHandler>(interceptor);
            var brokeredMessage = NimbusMessageFactory.Create("nullQueue", new ExceptingRequest()).Result;

            dispatcher.Dispatch(brokeredMessage).Wait();

            interceptor
                .Received()
                .OnRequestHandlerError(Arg.Any<ExceptingRequest>(), brokeredMessage, Arg.Any<Exception>());
        }
    }
}