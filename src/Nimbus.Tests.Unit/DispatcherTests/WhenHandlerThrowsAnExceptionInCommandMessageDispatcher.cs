using System;
using Nimbus.Interceptors.Inbound;
using Nimbus.Tests.Unit.DispatcherTests.Handlers;
using Nimbus.Tests.Unit.DispatcherTests.MessageContracts;
using NSubstitute;
using NUnit.Framework;

namespace Nimbus.Tests.Unit.DispatcherTests
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