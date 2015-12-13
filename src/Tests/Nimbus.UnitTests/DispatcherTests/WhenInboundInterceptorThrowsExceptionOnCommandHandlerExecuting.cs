using System;
using Nimbus.Infrastructure;
using Nimbus.Interceptors.Inbound;
using Nimbus.UnitTests.DispatcherTests.Handlers;
using Nimbus.UnitTests.DispatcherTests.MessageContracts;
using NSubstitute;
using NUnit.Framework;

namespace Nimbus.UnitTests.DispatcherTests
{
    public class WhenInboundInterceptorThrowsExceptionOnCommandHandlerExecuting
        : MessageDispatcherTestBase
    {
        [Test]
        public void TheExceptionIsBubbledBackThroughTheInterceptors()
        {
            var interceptor = Substitute.For<IInboundInterceptor>();
            interceptor
                .When(x => x.OnCommandHandlerExecuting(Arg.Any<EmptyCommand>(), Arg.Any<NimbusMessage>()))
                .Do(x =>
                {
                    throw new Exception("Ruh roh");
                });
            var dispatcher = GetCommandMessageDispatcher<EmptyCommand, EmptyCommandHandler>(interceptor);
            var brokeredMessage = NimbusMessageFactory.Create(new EmptyCommand()).Result;

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
                .OnCommandHandlerError(Arg.Any<EmptyCommand>(), brokeredMessage, Arg.Any<Exception>());
        }
    }
}
