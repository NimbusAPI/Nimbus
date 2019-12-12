﻿using System;
using Nimbus.Interceptors.Inbound;
using Nimbus.Tests.Unit.DispatcherTests.Handlers;
using Nimbus.Tests.Unit.DispatcherTests.MessageContracts;
using NSubstitute;
using NUnit.Framework;

namespace Nimbus.Tests.Unit.DispatcherTests
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