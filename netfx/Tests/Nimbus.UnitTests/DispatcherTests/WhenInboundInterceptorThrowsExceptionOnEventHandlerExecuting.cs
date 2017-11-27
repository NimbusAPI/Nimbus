﻿using System;
using Nimbus.Interceptors.Inbound;
using Nimbus.UnitTests.DispatcherTests.Handlers;
using Nimbus.UnitTests.DispatcherTests.MessageContracts;
using NSubstitute;
using NUnit.Framework;

namespace Nimbus.UnitTests.DispatcherTests
{
    public class WhenInboundInterceptorThrowsExceptionOnEventHandlerExecutin : MessageDispatcherTestBase
    {
        [Test]
        public void TheExceptionIsBubbledBackThroughTheInterceptors()
        {
            var interceptor = Substitute.For<IInboundInterceptor>();
            interceptor
                .When(x => x.OnEventHandlerExecuting(Arg.Any<EmptyEvent>(), Arg.Any<NimbusMessage>()))
                .Do(x => { throw new Exception("Ruh roh"); });
            var dispatcher = GetEventMessageDispatcher<EmptyEvent, EmptyEventHandler>(interceptor);
            var nimbusMessage = NimbusMessageFactory.Create("someQueue", new EmptyEvent()).Result;

            try
            {
                dispatcher.Dispatch(nimbusMessage).Wait();
            }
            catch (AggregateException)
            {
                // Dispatch rethrows the exception, don't care
            }

            interceptor
                .Received()
                .OnEventHandlerError(Arg.Any<EmptyEvent>(), nimbusMessage, Arg.Any<Exception>());
        }
    }
}