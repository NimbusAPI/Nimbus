using System;
using Nimbus.Interceptors.Inbound;
using Nimbus.UnitTests.DispatcherTests.Handlers;
using Nimbus.UnitTests.DispatcherTests.MessageContracts;
using NSubstitute;
using NUnit.Framework;

namespace Nimbus.UnitTests.DispatcherTests
{
    public class WhenInboundInterceptorThrowsExceptionOnRequestHandlerExecuting : MessageDispatcherTestBase
    {
        [Test]
        public void TheExceptionIsBubbledBackThroughTheInterceptors()
        {
            var interceptor = Substitute.For<IInboundInterceptor>();
            interceptor
                .When(x => x.OnRequestHandlerExecuting(Arg.Any<EmptyRequest>(), Arg.Any<NimbusMessage>()))
                .Do(x => { throw new Exception("Ruh roh"); });
            var dispatcher = GetRequestMessageDispatcher<EmptyRequest, EmptyResponse, EmptyRequestHandler>(interceptor);
            var nimbusMessage = NimbusMessageFactory.Create("someQueue", new EmptyRequest()).Result;

            dispatcher.Dispatch(nimbusMessage).Wait();

            interceptor
                .Received()
                .OnRequestHandlerError(Arg.Any<EmptyRequest>(), nimbusMessage, Arg.Any<Exception>());
        }
    }
}