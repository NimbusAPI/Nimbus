using Nimbus.Configuration.Settings;
using Nimbus.DependencyResolution;
using Nimbus.Handlers;
using Nimbus.Infrastructure;
using Nimbus.Infrastructure.BrokeredMessageServices.Serialization;
using Nimbus.Infrastructure.Commands;
using Nimbus.Infrastructure.Dispatching;
using Nimbus.Infrastructure.Events;
using Nimbus.Infrastructure.NimbusMessageServices;
using Nimbus.Infrastructure.PropertyInjection;
using Nimbus.Infrastructure.RequestResponse;
using Nimbus.Interceptors.Inbound;
using Nimbus.MessageContracts;
using Nimbus.Tests.Common;
using NSubstitute;

namespace Nimbus.UnitTests.DispatcherTests
{
    public abstract class MessageDispatcherTestBase
    {
        internal NimbusMessageFactory NimbusMessageFactory;
        internal TestHarnessTypeProvider TypeProvider;
        internal HandlerMapper HandlerMapper;

        protected MessageDispatcherTestBase()
        {
            var clock = Substitute.For<IClock>();
            var replyQueueNameSetting = new ReplyQueueNameSetting(
                new ApplicationNameSetting {Value = "TestApplication"},
                new InstanceNameSetting {Value = "TestInstance"});
            TypeProvider = new TestHarnessTypeProvider(new[] {GetType().Assembly}, new[] {GetType().Namespace});
            var serializer = new DataContractSerializer(TypeProvider);
            HandlerMapper = new HandlerMapper(TypeProvider);
            NimbusMessageFactory = new NimbusMessageFactory(new DefaultMessageTimeToLiveSetting(),
                                                            replyQueueNameSetting,
                                                            clock,
                                                            new DispatchContextManager());
        }

        internal RequestMessageDispatcher GetRequestMessageDispatcher<TRequest, TResponse, TRequestHandler>(IInboundInterceptor interceptor)
            where TRequest : IBusRequest<TRequest, TResponse>
            where TResponse : IBusResponse
            where TRequestHandler : IHandleRequest<TRequest, TResponse>, new()
        {
            var messagingFactory = Substitute.For<INimbusTransport>();
            var clock = Substitute.For<IClock>();
            var logger = Substitute.For<ILogger>();
            var dependencyResolver = Substitute.For<IDependencyResolver>();
            var scope = Substitute.For<IDependencyResolverScope>();
            scope.Resolve<TRequestHandler>().Returns(new TRequestHandler());
            dependencyResolver.CreateChildScope().Returns(scope);
            var inboundInterceptorFactory = Substitute.For<IInboundInterceptorFactory>();
            inboundInterceptorFactory.CreateInterceptors(Arg.Any<IDependencyResolverScope>(), Arg.Any<object>(), Arg.Any<object>(), Arg.Any<NimbusMessage>())
                                     .Returns(new[] {interceptor});

            var outboundInterceptorFactory = new NullOutboundInterceptorFactory();

            return new RequestMessageDispatcher(
                NimbusMessageFactory,
                clock,
                dependencyResolver,
                inboundInterceptorFactory,
                outboundInterceptorFactory,
                logger,
                messagingFactory,
                HandlerMapper.GetFullHandlerMap(typeof (IHandleRequest<,>)),
                new DefaultMessageLockDurationSetting(),
                Substitute.For<IPropertyInjector>());
        }

        internal CommandMessageDispatcher GetCommandMessageDispatcher<TCommand, TCommandHandler>(IInboundInterceptor interceptor)
            where TCommand : IBusCommand
            where TCommandHandler : IHandleCommand<TCommand>, new()
        {
            var clock = Substitute.For<IClock>();
            var logger = Substitute.For<ILogger>();
            var dependencyResolver = Substitute.For<IDependencyResolver>();
            var scope = Substitute.For<IDependencyResolverScope>();
            scope.Resolve<TCommandHandler>().Returns(new TCommandHandler());
            dependencyResolver.CreateChildScope().Returns(scope);
            var inboundInterceptorFactory = Substitute.For<IInboundInterceptorFactory>();
            inboundInterceptorFactory.CreateInterceptors(Arg.Any<IDependencyResolverScope>(), Arg.Any<object>(), Arg.Any<object>(), Arg.Any<NimbusMessage>())
                                     .Returns(new[] {interceptor});

            return new CommandMessageDispatcher(
                NimbusMessageFactory,
                clock,
                dependencyResolver,
                inboundInterceptorFactory,
                logger,
                HandlerMapper.GetFullHandlerMap(typeof (IHandleCommand<>)),
                new DefaultMessageLockDurationSetting(),
                Substitute.For<IPropertyInjector>());
        }

        internal EventMessageDispatcher GetEventMessageDispatcher<TEvent, TEventMessageHandler>(IInboundInterceptor interceptor)
            where TEvent : IBusEvent
            where TEventMessageHandler : IHandleMulticastEvent<TEvent>, new()
        {
            var clock = Substitute.For<IClock>();
            var logger = Substitute.For<ILogger>();
            var dependencyResolver = Substitute.For<IDependencyResolver>();
            var scope = Substitute.For<IDependencyResolverScope>();
            scope.Resolve<TEventMessageHandler>().Returns(new TEventMessageHandler());
            dependencyResolver.CreateChildScope().Returns(scope);
            var inboundInterceptorFactory = Substitute.For<IInboundInterceptorFactory>();
            inboundInterceptorFactory.CreateInterceptors(Arg.Any<IDependencyResolverScope>(), Arg.Any<object>(), Arg.Any<object>(), Arg.Any<NimbusMessage>())
                                     .Returns(new[] {interceptor});

            return new MulticastEventMessageDispatcher(
                NimbusMessageFactory,
                clock,
                dependencyResolver,
                inboundInterceptorFactory,
                HandlerMapper.GetFullHandlerMap(typeof (IHandleMulticastEvent<>)),
                new DefaultMessageLockDurationSetting(),
                Substitute.For<IPropertyInjector>(),
                logger);
        }
    }
}