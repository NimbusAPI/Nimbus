using System;
using System.Collections.Generic;
using Nimbus.Configuration.Settings;
using Nimbus.DependencyResolution;
using Nimbus.Extensions;
using Nimbus.Handlers;
using Nimbus.Infrastructure.Commands;
using Nimbus.Infrastructure.Events;
using Nimbus.Infrastructure.PropertyInjection;
using Nimbus.Infrastructure.RequestResponse;
using Nimbus.Infrastructure.TaskScheduling;
using Nimbus.Interceptors.Inbound;
using Nimbus.Interceptors.Outbound;

namespace Nimbus.Infrastructure
{
    internal class MessageDispatcherFactory : IMessageDispatcherFactory
    {
        private readonly IBrokeredMessageFactory _brokeredMessageFactory;
        private readonly IClock _clock;
        private readonly IDependencyResolver _dependencyResolver;
        private readonly IInboundInterceptorFactory _inboundInterceptorFactory;
        private readonly IOutboundInterceptorFactory _outboundInterceptorFactory;
        private readonly ILogger _logger;
        private readonly INimbusMessagingFactory _messagingFactory;
        private readonly DefaultMessageLockDurationSetting _defaultMessageLockDuration;
        private readonly NimbusTaskFactory _taskFactory;
        private readonly IPropertyInjector _propertyInjector;

        public MessageDispatcherFactory(IBrokeredMessageFactory brokeredMessageFactory,
                                        IClock clock,
                                        IDependencyResolver dependencyResolver,
                                        IInboundInterceptorFactory inboundInterceptorFactory,
                                        ILogger logger,
                                        INimbusMessagingFactory messagingFactory,
                                        IOutboundInterceptorFactory outboundInterceptorFactory,
                                        DefaultMessageLockDurationSetting defaultMessageLockDuration,
                                        NimbusTaskFactory taskFactory,
                                        IPropertyInjector propertyInjector)
        {
            _brokeredMessageFactory = brokeredMessageFactory;
            _clock = clock;
            _dependencyResolver = dependencyResolver;
            _inboundInterceptorFactory = inboundInterceptorFactory;
            _logger = logger;
            _messagingFactory = messagingFactory;
            _outboundInterceptorFactory = outboundInterceptorFactory;
            _defaultMessageLockDuration = defaultMessageLockDuration;
            _taskFactory = taskFactory;
            _propertyInjector = propertyInjector;
        }

        public IMessageDispatcher Create(Type openGenericHandlerType, IReadOnlyDictionary<Type, Type[]> handlerMap)
        {
            return BuildDispatcher(openGenericHandlerType, handlerMap);
        }

        private IMessageDispatcher BuildDispatcher(Type openGenericHandlerType, IReadOnlyDictionary<Type, Type[]> handlerMap)
        {
            if (openGenericHandlerType == typeof (IHandleCommand<>))
            {
                return new CommandMessageDispatcher(_brokeredMessageFactory,
                                                    _clock,
                                                    _dependencyResolver,
                                                    _inboundInterceptorFactory,
                                                    _logger,
                                                    handlerMap,
                                                    _defaultMessageLockDuration,
                                                    _taskFactory,
                                                    _propertyInjector);
            }

            if (openGenericHandlerType == typeof (IHandleCompetingEvent<>))
            {
                return new CompetingEventMessageDispatcher(_brokeredMessageFactory,
                                                           _clock,
                                                           _dependencyResolver,
                                                           _inboundInterceptorFactory,
                                                           handlerMap,
                                                           _defaultMessageLockDuration,
                                                           _taskFactory,
                                                           _propertyInjector,
                                                           _logger);
            }

            if (openGenericHandlerType == typeof (IHandleMulticastEvent<>))
            {
                return new MulticastEventMessageDispatcher(_brokeredMessageFactory,
                                                           _clock,
                                                           _dependencyResolver,
                                                           _inboundInterceptorFactory,
                                                           handlerMap,
                                                           _defaultMessageLockDuration,
                                                           _taskFactory,
                                                           _propertyInjector,
                                                           _logger);
            }

            if (openGenericHandlerType == typeof (IHandleRequest<,>))
            {
                return new RequestMessageDispatcher(_brokeredMessageFactory,
                                                    _clock,
                                                    _dependencyResolver,
                                                    _inboundInterceptorFactory,
                                                    _outboundInterceptorFactory,
                                                    _logger,
                                                    _messagingFactory,
                                                    handlerMap,
                                                    _defaultMessageLockDuration,
                                                    _taskFactory);
            }

            if (openGenericHandlerType == typeof (IHandleMulticastRequest<,>))
            {
                return new MulticastRequestMessageDispatcher(_brokeredMessageFactory,
                                                             _clock,
                                                             _dependencyResolver,
                                                             _inboundInterceptorFactory,
                                                             _logger,
                                                             _messagingFactory,
                                                             _outboundInterceptorFactory,
                                                             handlerMap,
                                                             _defaultMessageLockDuration,
                                                             _taskFactory);
            }

            throw new NotSupportedException("There is no dispatcher for the handler type {0}.".FormatWith(openGenericHandlerType.FullName));
        }
    }
}