using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nimbus.Exceptions;
using Nimbus.Extensions;
using Nimbus.Infrastructure.Filtering;
using Nimbus.InfrastructureContracts;
using Nimbus.InfrastructureContracts.DependencyResolution;
using Nimbus.Interceptors.Inbound;
using Nimbus.MessageContracts;

namespace Nimbus.Infrastructure.Events
{
    internal abstract class EventMessageDispatcher : IMessageDispatcher
    {
        private readonly IDependencyResolver _dependencyResolver;
        private readonly IReadOnlyDictionary<Type, Type[]> _handlerMap;
        private readonly IInboundInterceptorFactory _inboundInterceptorFactory;
        private readonly IFilterConditionProvider _filterConditionProvider;
        private readonly ILogger _logger;

        protected EventMessageDispatcher(IDependencyResolver dependencyResolver,
                                         IReadOnlyDictionary<Type, Type[]> handlerMap,
                                         IInboundInterceptorFactory inboundInterceptorFactory,
                                         IFilterConditionProvider filterConditionProvider,
                                         ILogger logger)
        {
            _dependencyResolver = dependencyResolver;
            _handlerMap = handlerMap;
            _inboundInterceptorFactory = inboundInterceptorFactory;
            _filterConditionProvider = filterConditionProvider;
            _logger = logger;
        }

        public async Task Dispatch(NimbusMessage message)
        {
            var busEvent = message.Payload;
            var messageType = busEvent.GetType();

            // There should only ever be a single event handler associated with this dispatcher
            var handlerType = _handlerMap.GetSingleHandlerTypeFor(messageType);
            await (Task) Dispatch((dynamic) busEvent, message, handlerType);
        }

        protected abstract object CreateHandlerFromScope<TBusEvent>(IDependencyResolverScope scope, TBusEvent busEvent, Type handlerType, NimbusMessage nimbusMessage)
            where TBusEvent : IBusEvent;

        protected abstract Task DispatchToHandleMethod<TBusEvent>(TBusEvent busEvent, object handler)
            where TBusEvent : IBusEvent;

        private async Task Dispatch<TBusEvent>(TBusEvent busEvent, NimbusMessage nimbusMessage, Type handlerType) where TBusEvent : IBusEvent
        {
            var subscriptionFilter = _filterConditionProvider.GetFilterConditionFor(handlerType);
            if (!nimbusMessage.MatchesFilter(subscriptionFilter))
            {
                _logger.Debug("Message {MessageId} does not match the subscription filter for {HandlerType}. Dropping it immediately.", nimbusMessage.MessageId, handlerType);
                return;
            }

            using (var scope = _dependencyResolver.CreateChildScope())
            {
                var handler = CreateHandlerFromScope(scope, busEvent, handlerType, nimbusMessage);

                var interceptors = _inboundInterceptorFactory.CreateInterceptors(scope, handler, busEvent, nimbusMessage);

                Exception exception;
                try
                {
                    foreach (var interceptor in interceptors)
                    {
                        _logger.Debug("Executing OnEventHandlerExecuting on {0} for message [MessageType:{1}, MessageId:{2}, CorrelationId:{3}]",
                                      interceptor.GetType().FullName,
                                      nimbusMessage.SafelyGetBodyTypeNameOrDefault(),
                                      nimbusMessage.MessageId,
                                      nimbusMessage.CorrelationId);

                        await interceptor.OnEventHandlerExecuting(busEvent, nimbusMessage);

                        _logger.Debug("Executed OnEventHandlerExecuting on {0} for message [MessageType:{1}, MessageId:{2}, CorrelationId:{3}]",
                                      interceptor.GetType().FullName,
                                      nimbusMessage.SafelyGetBodyTypeNameOrDefault(),
                                      nimbusMessage.MessageId,
                                      nimbusMessage.CorrelationId);
                    }

                    await DispatchToHandleMethod(busEvent, handler);

                    foreach (var interceptor in interceptors.Reverse())
                    {
                        _logger.Debug("Executing OnEventHandlerSuccess on {0} for message [MessageType:{1}, MessageId:{2}, CorrelationId:{3}]",
                                      interceptor.GetType().FullName,
                                      nimbusMessage.SafelyGetBodyTypeNameOrDefault(),
                                      nimbusMessage.MessageId,
                                      nimbusMessage.CorrelationId);

                        await interceptor.OnEventHandlerSuccess(busEvent, nimbusMessage);

                        _logger.Debug("Executed OnEventHandlerSuccess on {0} for message [MessageType:{1}, MessageId:{2}, CorrelationId:{3}]",
                                      interceptor.GetType().FullName,
                                      nimbusMessage.SafelyGetBodyTypeNameOrDefault(),
                                      nimbusMessage.MessageId,
                                      nimbusMessage.CorrelationId);
                    }
                    return;
                }
                catch (Exception exc)
                {
                    exception = exc;
                }

                foreach (var interceptor in interceptors.Reverse())
                {
                    _logger.Debug("Executing OnEventHandlerError on {0} for message [MessageType:{1}, MessageId:{2}, CorrelationId:{3}]",
                                  interceptor.GetType().FullName,
                                  nimbusMessage.SafelyGetBodyTypeNameOrDefault(),
                                  nimbusMessage.MessageId,
                                  nimbusMessage.CorrelationId);

                    await interceptor.OnEventHandlerError(busEvent, nimbusMessage, exception);

                    _logger.Debug("Executed OnEventHandlerError on {0} for message [MessageType:{1}, MessageId:{2}, CorrelationId:{3}]",
                                  interceptor.GetType().FullName,
                                  nimbusMessage.SafelyGetBodyTypeNameOrDefault(),
                                  nimbusMessage.MessageId,
                                  nimbusMessage.CorrelationId);
                }

                _logger.Debug("Failed to dispatch EventMessage for message [MessageType:{0}, MessageId:{1}, CorrelationId:{2}]",
                              nimbusMessage.SafelyGetBodyTypeNameOrDefault(),
                              nimbusMessage.MessageId,
                              nimbusMessage.CorrelationId);
                throw new DispatchFailedException("Failed to dispatch EventMessage", exception);
            }
        }
    }
}