using Castle.DynamicProxy;
using Castle.DynamicProxy.Generators.Emitters.SimpleAST;
using Castle.MicroKernel.ModelBuilder.Descriptors;
using Nimbus.Configuration.LargeMessages.Settings;
using Nimbus.Configuration.Settings;
using Nimbus.DependencyResolution;
using Nimbus.Handlers;
using Nimbus.Infrastructure;
using Nimbus.Infrastructure.BrokeredMessageServices;
using Nimbus.Infrastructure.BrokeredMessageServices.Compression;
using Nimbus.Infrastructure.BrokeredMessageServices.LargeMessages;
using Nimbus.Infrastructure.BrokeredMessageServices.Serialization;
using Nimbus.Infrastructure.Commands;
using Nimbus.Infrastructure.Events;
using Nimbus.Infrastructure.RequestResponse;
using Nimbus.Interceptors.Inbound;
using Nimbus.MessageContracts;
using NSubstitute;

namespace Nimbus.UnitTests.DispatcherTests
{
    public abstract class MessageDispatcherTestBase
    {
        internal BrokeredMessageFactory BrokeredMessageFactory;

        protected MessageDispatcherTestBase()
        {
            var clock = Substitute.For<IClock>();
            var serializer = new DataContractSerializer();
            var replyQueueNameSetting = new ReplyQueueNameSetting(
                new ApplicationNameSetting {Value = "TestApplication"},
                new InstanceNameSetting {Value = "TestInstance"});
            BrokeredMessageFactory = new BrokeredMessageFactory(new MaxLargeMessageSizeSetting(),
                                                                new MaxSmallMessageSizeSetting(),
                                                                replyQueueNameSetting,
                                                                clock,
                                                                new NullCompressor(),
                                                                new NullDependencyResolver(),
                                                                new UnsupportedLargeMessageBodyStore(),
                                                                new NullOutboundInterceptorFactory(),
                                                                serializer);
        }

        internal RequestMessageDispatcher GetRequestMessageDispatcher<TRequest, TResponse, TRequestHandler>(IInboundInterceptor interceptor) 
            where TRequest : IBusRequest<TRequest, TResponse> 
            where TResponse : IBusResponse
            where TRequestHandler : IHandleRequest<TRequest, TResponse>, new()
        {
            var messagingFactory = Substitute.For<INimbusMessagingFactory>();
            var clock = Substitute.For<IClock>();
            var logger = Substitute.For<ILogger>();
            var dependencyResolver = Substitute.For<IDependencyResolver>();
            var scope = Substitute.For<IDependencyResolverScope>();
            scope.Resolve<IHandleRequest<TRequest, TResponse>>(Arg.Any<string>()).Returns(new TRequestHandler());
            dependencyResolver.CreateChildScope().Returns(scope);
            var inboundInterceptorFactory = Substitute.For<IInboundInterceptorFactory>();
            inboundInterceptorFactory.CreateInterceptors(Arg.Any<IDependencyResolverScope>(), Arg.Any<object>(), Arg.Any<object>())
                                     .Returns(new[] { interceptor });

            return new RequestMessageDispatcher(
                messagingFactory,
                BrokeredMessageFactory,
                inboundInterceptorFactory,
                typeof (TRequest),
                clock,
                logger,
                dependencyResolver,
                typeof (TRequestHandler));
        }

        internal CommandMessageDispatcher GetCommandMessageDispatcher<TCommand, TCommandHandler>(IInboundInterceptor interceptor)
            where TCommand : IBusCommand
            where TCommandHandler : IHandleCommand<TCommand>, new()
        {
            var clock = Substitute.For<IClock>();
            var logger = Substitute.For<ILogger>();
            var dependencyResolver = Substitute.For<IDependencyResolver>();
            var scope = Substitute.For < IDependencyResolverScope>();
            scope.Resolve<IHandleCommand<TCommand>>(Arg.Any<string>()).Returns(new TCommandHandler());
            dependencyResolver.CreateChildScope().Returns(scope);
            var inboundInterceptorFactory = Substitute.For<IInboundInterceptorFactory>();
            inboundInterceptorFactory.CreateInterceptors(Arg.Any<IDependencyResolverScope>(), Arg.Any<object>(), Arg.Any<object>())
                                     .Returns(new[] { interceptor });

            return new CommandMessageDispatcher(
                dependencyResolver,
                inboundInterceptorFactory,
                BrokeredMessageFactory,
                typeof (TCommand),
                clock,
                typeof (TCommandHandler),
                logger);
        }

        internal EventMessageDispatcher GetEventMessageDispatcher<TEvent, TEventMessageHandler>(IInboundInterceptor interceptor)
            where TEvent : IBusEvent
            where TEventMessageHandler : IHandleMulticastEvent<TEvent>, new()
        {
            var clock = Substitute.For<IClock>();
            var dependencyResolver = Substitute.For<IDependencyResolver>();
            var scope = Substitute.For<IDependencyResolverScope>();
            scope.Resolve<IHandleMulticastEvent<TEvent>>(Arg.Any<string>()).Returns(new TEventMessageHandler());
            dependencyResolver.CreateChildScope().Returns(scope);
            var inboundInterceptorFactory = Substitute.For<IInboundInterceptorFactory>();
            inboundInterceptorFactory.CreateInterceptors(Arg.Any<IDependencyResolverScope>(), Arg.Any<object>(), Arg.Any<object>())
                                     .Returns(new[] {interceptor});

            return new MulticastEventMessageDispatcher(
                dependencyResolver,
                BrokeredMessageFactory,
                inboundInterceptorFactory,
                typeof (TEventMessageHandler),
                clock,
                typeof (TEvent));
        }
    }
}