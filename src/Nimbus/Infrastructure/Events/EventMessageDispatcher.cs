using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Nimbus.DependencyResolution;
using Nimbus.Exceptions;
using Nimbus.Extensions;
using Nimbus.Handlers;
using Nimbus.Interceptors.Inbound;
using Nimbus.MessageContracts;

namespace Nimbus.Infrastructure.Events
{
    internal abstract class EventMessageDispatcher : IMessageDispatcher
    {
        private readonly IDependencyResolver _dependencyResolver;
        private readonly IReadOnlyDictionary<Type, Type[]> _handlerMap;
        private readonly IBrokeredMessageFactory _brokeredMessageFactory;
        private readonly IClock _clock;
        private readonly IInboundInterceptorFactory _inboundInterceptorFactory;
        private readonly ILogger _logger;

        protected EventMessageDispatcher(IBrokeredMessageFactory brokeredMessageFactory,
            IClock clock,
            IDependencyResolver dependencyResolver,
            IReadOnlyDictionary<Type, Type[]> handlerMap,
            IInboundInterceptorFactory inboundInterceptorFactory,
            ILogger logger)
        {
            _brokeredMessageFactory = brokeredMessageFactory;
            _clock = clock;
            _dependencyResolver = dependencyResolver;
            _handlerMap = handlerMap;
            _inboundInterceptorFactory = inboundInterceptorFactory;
            _logger = logger;
        }

        public async Task Dispatch(BrokeredMessage message)
        {
            var busEvent = await _brokeredMessageFactory.GetBody(message);
            var messageType = busEvent.GetType();

            // There should only ever be a single event handler associated with this dispatcher
            var handlerType = _handlerMap.GetSingleHandlerTypeFor(messageType);
            await (Task) Dispatch((dynamic) busEvent, message, handlerType);
        }

        protected abstract object CreateHandlerFromScope<TBusEvent>(IDependencyResolverScope scope, TBusEvent busEvent, Type handlerType) where TBusEvent : IBusEvent;
        protected abstract Task DispatchToHandleMethod<TBusEvent>(TBusEvent busEvent, object handler) where TBusEvent : IBusEvent;

        private async Task Dispatch<TBusEvent>(TBusEvent busEvent, BrokeredMessage message, Type handlerType) where TBusEvent : IBusEvent
        {
            using (var scope = _dependencyResolver.CreateChildScope())
            {
                var handler = CreateHandlerFromScope(scope, busEvent, handlerType);
                var interceptors = _inboundInterceptorFactory.CreateInterceptors(scope, handler, busEvent);

                Exception exception;
                try
                {
                    foreach (var interceptor in interceptors)
                    {
                        _logger.Debug("Executing OnEventHandlerExecuting on {0} for message [MessageType:{1}, MessageId:{2}, CorrelationId:{3}]",
                            interceptor.GetType().FullName,
                            message.SafelyGetBodyTypeNameOrDefault(),
                            message.MessageId,
                            message.CorrelationId);

                        await interceptor.OnEventHandlerExecuting(busEvent, message);

                        _logger.Debug("Executed OnEventHandlerExecuting on {0} for message [MessageType:{1}, MessageId:{2}, CorrelationId:{3}]",
                            interceptor.GetType().FullName,
                            message.SafelyGetBodyTypeNameOrDefault(),
                            message.MessageId,
                            message.CorrelationId);
                    }

                    var handlerTask = DispatchToHandleMethod(busEvent, handler);
                    var wrapper = new LongLivedTaskWrapper(_logger, handlerTask, handler as ILongRunningTask, message, _clock);
                    await wrapper.AwaitCompletion();

                    foreach (var interceptor in interceptors.Reverse())
                    {
                        _logger.Debug("Executing OnEventHandlerSuccess on {0} for message [MessageType:{1}, MessageId:{2}, CorrelationId:{3}]",
                        interceptor.GetType().FullName,
                        message.SafelyGetBodyTypeNameOrDefault(),
                        message.MessageId,
                        message.CorrelationId);

                        await interceptor.OnEventHandlerSuccess(busEvent, message);

                        _logger.Debug("Executed OnEventHandlerSuccess on {0} for message [MessageType:{1}, MessageId:{2}, CorrelationId:{3}]",
                        interceptor.GetType().FullName,
                        message.SafelyGetBodyTypeNameOrDefault(),
                        message.MessageId,
                        message.CorrelationId);
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
                        message.SafelyGetBodyTypeNameOrDefault(),
                        message.MessageId,
                        message.CorrelationId);

                    await interceptor.OnEventHandlerError(busEvent, message, exception);

                    _logger.Debug("Executed OnEventHandlerError on {0} for message [MessageType:{1}, MessageId:{2}, CorrelationId:{3}]",
                        interceptor.GetType().FullName,
                        message.SafelyGetBodyTypeNameOrDefault(),
                        message.MessageId,
                        message.CorrelationId);
                }

                _logger.Debug("Failed to dispatch EventMessage for message [MessageType:{0}, MessageId:{1}, CorrelationId:{2}]",
                    message.SafelyGetBodyTypeNameOrDefault(),
                    message.MessageId,
                    message.CorrelationId);
                throw new DispatchFailedException("Failed to dispatch EventMessage", exception);
            }
        }
    }
}