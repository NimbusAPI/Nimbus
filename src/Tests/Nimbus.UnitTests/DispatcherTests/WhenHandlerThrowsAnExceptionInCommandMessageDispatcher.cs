using System;
using Nimbus.Interceptors.Inbound;
using Nimbus.UnitTests.DispatcherTests.Handlers;
using Nimbus.UnitTests.DispatcherTests.MessageContracts;
using NSubstitute;
using NUnit.Framework;

namespace Nimbus.UnitTests.DispatcherTests
{
    public class WhenHandlerThrowsAnExceptionInCommandMessageDispatcher
        : MessageDispatcherTestBase
    {
        [Test]
        public void TheExceptionIsBubbledThroughTheInterceptors()
        {
            var interceptor = Substitute.For<IInboundInterceptor>();
            var dispatcher = GetCommandMessageDispatcher<ExceptingCommand, ExceptingCommandHandler>(interceptor);
            var brokeredMessage = NimbusMessageFactory.Create("someQueue", new ExceptingCommand()).Result;

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
                .OnCommandHandlerError(Arg.Any<ExceptingCommand>(), brokeredMessage, Arg.Any<Exception>());
        }
    }
}