using System;
using Microsoft.ServiceBus.Messaging;
using Nimbus.Interceptors.Inbound;
using Nimbus.UnitTests.DispatcherTests.Handlers;
using Nimbus.UnitTests.DispatcherTests.MessageContracts;
using NSubstitute;
using NUnit.Framework;

namespace Nimbus.UnitTests.DispatcherTests
{
    public class WhenInboundInterceptorThrowsExceptionOnEventHandlerExecuting
        : MessageDispatcherTestBase
    {
        [Test]
        public void TheExceptionIsBubbledBackThroughTheInterceptors()
        {
            var interceptor = Substitute.For<IInboundInterceptor>();
            interceptor
                .When(x => x.OnEventHandlerExecuting(Arg.Any<EmptyEvent>(), Arg.Any<BrokeredMessage>()))
                .Do(x => { throw new Exception("Ruh roh"); });
            var dispatcher = GetEventMessageDispatcher<EmptyEvent, EmptyEventHandler>(interceptor);
            var brokeredMessage = BrokeredMessageFactory.Create(new EmptyEvent()).Result;

            try
            {
                dispatcher.Dispatch(brokeredMessage).Wait();
            }
            catch (AggregateException)
            {
                // Dispatch rethrows the exception, don't care
            }

            interceptor
                .Received()
                .OnEventHandlerError(Arg.Any<EmptyEvent>(), brokeredMessage, Arg.Any<Exception>());
        }
    }
}