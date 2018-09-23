using System;
using Nimbus.Interceptors.Inbound;
using Nimbus.UnitTests.DispatcherTests.Handlers;
using Nimbus.UnitTests.DispatcherTests.MessageContracts;
using NSubstitute;
using NUnit.Framework;

namespace Nimbus.UnitTests.DispatcherTests
{
    public class WhenHandlerThrowsAnExceptionInEventMessageDispatcher : MessageDispatcherTestBase
    {
        [Test]
        public void TheExceptionIsBubbledThroughTheInterceptors()
        {
            var interceptor = Substitute.For<IInboundInterceptor>();
            var dispatcher = GetEventMessageDispatcher<ExceptingEvent, ExceptingEventHandler>(interceptor);
            var nimbusMessage = NimbusMessageFactory.Create("someQueue", new ExceptingEvent()).Result;

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
                .OnEventHandlerError(Arg.Any<ExceptingEvent>(), nimbusMessage, Arg.Any<Exception>());
        }
    }
}