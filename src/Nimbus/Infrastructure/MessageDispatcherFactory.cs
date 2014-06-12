using System;
using System.Collections.Generic;
using Nimbus.DependencyResolution;
using Nimbus.Extensions;
using Nimbus.Handlers;
using Nimbus.Infrastructure.Commands;
using Nimbus.Infrastructure.Events;
using Nimbus.Infrastructure.RequestResponse;
using Nimbus.Interceptors.Inbound;

namespace Nimbus.Infrastructure
{
    internal class MessageDispatcherFactory : IMessageDispatcherFactory
    {
        private readonly IBrokeredMessageFactory _brokeredMessageFactory;
        private readonly IClock _clock;
        private readonly IDependencyResolver _dependencyResolver;
        private readonly IInboundInterceptorFactory _inboundInterceptorFactory;
        private readonly ILogger _logger;
        private readonly INimbusMessagingFactory _messagingFactory;

        public MessageDispatcherFactory(IBrokeredMessageFactory brokeredMessageFactory,
                                        IClock clock,
                                        IDependencyResolver dependencyResolver,
                                        IInboundInterceptorFactory inboundInterceptorFactory,
                                        ILogger logger,
                                        INimbusMessagingFactory messagingFactory)
        {
            _brokeredMessageFactory = brokeredMessageFactory;
            _clock = clock;
            _dependencyResolver = dependencyResolver;
            _inboundInterceptorFactory = inboundInterceptorFactory;
            _logger = logger;
            _messagingFactory = messagingFactory;
        }

        public IMessageDispatcher Create(Type handlerType, IReadOnlyDictionary<Type, Type> handlerMap)
        {
            return BuildDispatcher(handlerType, handlerMap);
        }

        private IMessageDispatcher BuildDispatcher(Type handlerType, IReadOnlyDictionary<Type, Type> handlerMap)
        {
            if (handlerType == typeof(IHandleCommand<>))
            {
                return new CommandMessageDispatcher(_brokeredMessageFactory,
                                                    _clock,
                                                    _dependencyResolver,
                                                    _inboundInterceptorFactory,
                                                    _logger,
                                                    handlerMap);
            }

            if (handlerType == typeof (IHandleCompetingEvent<>))
            {
                return new CompetingEventMessageDispatcher(_brokeredMessageFactory,
                                                           _clock,
                                                           _dependencyResolver,
                                                           _inboundInterceptorFactory,
                                                           handlerMap);
            }

            if (handlerType == typeof (IHandleMulticastEvent<>))
            {
                return new MulticastEventMessageDispatcher(_brokeredMessageFactory,
                                                           _clock,
                                                           _dependencyResolver,
                                                           _inboundInterceptorFactory,
                                                           handlerMap);
            }

            if (handlerType == typeof (IHandleRequest<,>))
            {
                return new RequestMessageDispatcher(_brokeredMessageFactory,
                                                    _clock,
                                                    _dependencyResolver,
                                                    _inboundInterceptorFactory,
                                                    _logger,
                                                    _messagingFactory,
                                                    handlerMap);
            }

            if (handlerType == typeof (IHandleMulticastRequest<,>))
            {
                return new MulticastRequestMessageDispatcher(_brokeredMessageFactory,
                                                             _clock,
                                                             _dependencyResolver,
                                                             _inboundInterceptorFactory,
                                                             _logger,
                                                             _messagingFactory,
                                                             handlerMap);
            }

            throw new NotSupportedException(
                "There is no dispatcher for the handler type {0}."
                    .FormatWith(handlerType.FullName));
        }
    }
}