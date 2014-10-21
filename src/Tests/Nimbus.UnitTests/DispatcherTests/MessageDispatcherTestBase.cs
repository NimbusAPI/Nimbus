using System;
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
using Nimbus.Infrastructure.Dispatching;
using Nimbus.Infrastructure.Events;
using Nimbus.Infrastructure.PropertyInjection;
using Nimbus.Infrastructure.RequestResponse;
using Nimbus.Infrastructure.TaskScheduling;
using Nimbus.Interceptors.Inbound;
using Nimbus.MessageContracts;
using Nimbus.Tests.Common;
using NSubstitute;

namespace Nimbus.UnitTests.DispatcherTests
{
    public abstract class MessageDispatcherTestBase
    {
        internal BrokeredMessageFactory BrokeredMessageFactory;
        internal TestHarnessTypeProvider TypeProvider;
        internal HandlerMapper HandlerMapper;
        private INimbusTaskFactory _taskFactory;

        protected MessageDispatcherTestBase()
        {
            var clock = Substitute.For<IClock>();
            var replyQueueNameSetting = new ReplyQueueNameSetting(
                new ApplicationNameSetting {Value = "TestApplication"},
                new InstanceNameSetting {Value = "TestInstance"});
            TypeProvider = new TestHarnessTypeProvider(new[] {GetType().Assembly}, new[] {GetType().Namespace});
            var serializer = new DataContractSerializer(TypeProvider);
            HandlerMapper = new HandlerMapper(TypeProvider);
            BrokeredMessageFactory = new BrokeredMessageFactory(new MaxLargeMessageSizeSetting(),
                                                                new MaxSmallMessageSizeSetting(),
                                                                replyQueueNameSetting,
                                                                clock,
                                                                new NullCompressor(),
                                                                new DispatchContextManager(),
                                                                new UnsupportedLargeMessageBodyStore(),
                                                                serializer,
                                                                TypeProvider);
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
            _taskFactory = new NimbusTaskFactory(new MaximumThreadPoolThreadsSetting(), new MinimumThreadPoolThreadsSetting(), logger);
            scope.Resolve<IHandleRequest<TRequest, TResponse>>(Arg.Any<string>()).Returns(new TRequestHandler());
            dependencyResolver.CreateChildScope().Returns(scope);
            var inboundInterceptorFactory = Substitute.For<IInboundInterceptorFactory>();
            inboundInterceptorFactory.CreateInterceptors(Arg.Any<IDependencyResolverScope>(), Arg.Any<object>(), Arg.Any<object>())
                                     .Returns(new[] {interceptor});

            var outboundInterceptorFactory = new NullOutboundInterceptorFactory();

            return new RequestMessageDispatcher(
                BrokeredMessageFactory,
                clock,
                dependencyResolver,
                inboundInterceptorFactory,
                outboundInterceptorFactory,
                logger,
                messagingFactory,
                HandlerMapper.GetFullHandlerMap(typeof (IHandleRequest<,>)),
                new DefaultMessageLockDurationSetting(),
                _taskFactory);
        }

        internal CommandMessageDispatcher GetCommandMessageDispatcher<TCommand, TCommandHandler>(IInboundInterceptor interceptor)
            where TCommand : IBusCommand
            where TCommandHandler : IHandleCommand<TCommand>, new()
        {
            var clock = Substitute.For<IClock>();
            var logger = Substitute.For<ILogger>();
            var dependencyResolver = Substitute.For<IDependencyResolver>();
            var scope = Substitute.For<IDependencyResolverScope>();
            scope.Resolve<IHandleCommand<TCommand>>(Arg.Any<string>()).Returns(new TCommandHandler());
            dependencyResolver.CreateChildScope().Returns(scope);
            var inboundInterceptorFactory = Substitute.For<IInboundInterceptorFactory>();
            inboundInterceptorFactory.CreateInterceptors(Arg.Any<IDependencyResolverScope>(), Arg.Any<object>(), Arg.Any<object>())
                                     .Returns(new[] {interceptor});

            return new CommandMessageDispatcher(
                BrokeredMessageFactory,
                clock,
                dependencyResolver,
                inboundInterceptorFactory,
                logger,
                HandlerMapper.GetFullHandlerMap(typeof (IHandleCommand<>)),
                new DefaultMessageLockDurationSetting(),
                new NimbusTaskFactory(new MaximumThreadPoolThreadsSetting(), new MinimumThreadPoolThreadsSetting(), logger),
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
            scope.Resolve<IHandleMulticastEvent<TEvent>>(Arg.Any<string>()).Returns(new TEventMessageHandler());
            dependencyResolver.CreateChildScope().Returns(scope);
            var inboundInterceptorFactory = Substitute.For<IInboundInterceptorFactory>();
            inboundInterceptorFactory.CreateInterceptors(Arg.Any<IDependencyResolverScope>(), Arg.Any<object>(), Arg.Any<object>())
                                     .Returns(new[] {interceptor});

            return new MulticastEventMessageDispatcher(
                BrokeredMessageFactory,
                clock,
                dependencyResolver,
                inboundInterceptorFactory,
                HandlerMapper.GetFullHandlerMap(typeof (IHandleMulticastEvent<>)),
                new DefaultMessageLockDurationSetting(),
                new NimbusTaskFactory(new MaximumThreadPoolThreadsSetting(), new MinimumThreadPoolThreadsSetting(), logger),
                Substitute.For<IPropertyInjector>(),
                logger);
        }
    }
}