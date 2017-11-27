using System;
using System.Collections.Generic;
using Nimbus.DependencyResolution;
using Nimbus.Extensions;
using Nimbus.Handlers;
using Nimbus.Infrastructure.Commands;
using Nimbus.Infrastructure.Events;
using Nimbus.Infrastructure.Filtering;
using Nimbus.Infrastructure.PropertyInjection;
using Nimbus.Infrastructure.RequestResponse;
using Nimbus.Interceptors.Inbound;
using Nimbus.Interceptors.Outbound;

namespace Nimbus.Infrastructure
{
    internal class MessageDispatcherFactory : IMessageDispatcherFactory
    {
        private readonly INimbusMessageFactory _nimbusMessageFactory;
        private readonly IDependencyResolver _dependencyResolver;
        private readonly IInboundInterceptorFactory _inboundInterceptorFactory;
        private readonly IOutboundInterceptorFactory _outboundInterceptorFactory;
        private readonly ILogger _logger;
        private readonly INimbusTransport _transport;
        private readonly IPropertyInjector _propertyInjector;
        private readonly IFilterConditionProvider _filterConditonProvider;

        public MessageDispatcherFactory(IDependencyResolver dependencyResolver,
                                        IFilterConditionProvider filterConditonProvider,
                                        IInboundInterceptorFactory inboundInterceptorFactory,
                                        ILogger logger,
                                        INimbusMessageFactory nimbusMessageFactory,
                                        INimbusTransport transport,
                                        IOutboundInterceptorFactory outboundInterceptorFactory,
                                        IPropertyInjector propertyInjector)
        {
            _nimbusMessageFactory = nimbusMessageFactory;
            _dependencyResolver = dependencyResolver;
            _inboundInterceptorFactory = inboundInterceptorFactory;
            _logger = logger;
            _transport = transport;
            _outboundInterceptorFactory = outboundInterceptorFactory;
            _propertyInjector = propertyInjector;
            _filterConditonProvider = filterConditonProvider;
        }

        public IMessageDispatcher Create(Type openGenericHandlerType, IReadOnlyDictionary<Type, Type[]> handlerMap)
        {
            return BuildDispatcher(openGenericHandlerType, handlerMap);
        }

        private IMessageDispatcher BuildDispatcher(Type openGenericHandlerType, IReadOnlyDictionary<Type, Type[]> handlerMap)
        {
            if (openGenericHandlerType == typeof(IHandleCommand<>))
            {
                return new CommandMessageDispatcher(_dependencyResolver,
                                                    _inboundInterceptorFactory,
                                                    _logger,
                                                    handlerMap,
                                                    _propertyInjector);
            }

            if (openGenericHandlerType == typeof(IHandleCompetingEvent<>))
            {
                return new CompetingEventMessageDispatcher(_dependencyResolver,
                                                           _inboundInterceptorFactory,
                                                           handlerMap,
                                                           _propertyInjector,
                                                           _logger,
                                                           _filterConditonProvider);
            }

            if (openGenericHandlerType == typeof(IHandleMulticastEvent<>))
            {
                return new MulticastEventMessageDispatcher(_dependencyResolver,
                                                           _inboundInterceptorFactory,
                                                           handlerMap,
                                                           _propertyInjector,
                                                           _logger,
                                                           _filterConditonProvider);
            }

            if (openGenericHandlerType == typeof(IHandleRequest<,>))
            {
                return new RequestMessageDispatcher(_nimbusMessageFactory,
                                                    _dependencyResolver,
                                                    _inboundInterceptorFactory,
                                                    _outboundInterceptorFactory,
                                                    _logger,
                                                    _transport,
                                                    handlerMap,
                                                    _propertyInjector);
            }

            if (openGenericHandlerType == typeof(IHandleMulticastRequest<,>))
            {
                return new MulticastRequestMessageDispatcher(_nimbusMessageFactory,
                                                             _dependencyResolver,
                                                             _inboundInterceptorFactory,
                                                             _logger,
                                                             _transport,
                                                             _outboundInterceptorFactory,
                                                             handlerMap,
                                                             _propertyInjector,
                                                             _filterConditonProvider);
            }

            throw new NotSupportedException("There is no dispatcher for the handler type {0}.".FormatWith(openGenericHandlerType.FullName));
        }
    }
}