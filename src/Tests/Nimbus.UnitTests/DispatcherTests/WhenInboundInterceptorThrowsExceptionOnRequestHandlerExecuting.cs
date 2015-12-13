using System;
using Microsoft.ServiceBus.Messaging;
using Nimbus.Infrastructure;
using Nimbus.Interceptors.Inbound;
using Nimbus.UnitTests.DispatcherTests.Handlers;
using Nimbus.UnitTests.DispatcherTests.MessageContracts;
using NSubstitute;
using NUnit.Framework;

namespace Nimbus.UnitTests.DispatcherTests
{
    public class WhenInboundInterceptorThrowsExceptionOnRequestHandlerExecuting
        : MessageDispatcherTestBase
    {
        [Test]
        public void TheExceptionIsBubbledBackThroughTheInterceptors()
        {
            var interceptor = Substitute.For<IInboundInterceptor>();
            interceptor
                .When(x => x.OnRequestHandlerExecuting(Arg.Any<EmptyRequest>(), Arg.Any<NimbusMessage>()))
                .Do(x => { throw new Exception("Ruh roh"); });
            var dispatcher = GetRequestMessageDispatcher<EmptyRequest, EmptyResponse, EmptyRequestHandler>(interceptor);
            var brokeredMessage = NimbusMessageFactory.Create(new EmptyRequest()).Result;

            dispatcher.Dispatch(brokeredMessage).Wait();

            interceptor
                .Received()
                .OnRequestHandlerError(Arg.Any<EmptyRequest>(), brokeredMessage, Arg.Any<Exception>());
        }
    }
}