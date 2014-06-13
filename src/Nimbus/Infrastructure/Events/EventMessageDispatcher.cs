using System;
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
        private readonly IBrokeredMessageFactory _brokeredMessageFactory;
        private readonly IClock _clock;
        private readonly IInboundInterceptorFactory _inboundInterceptorFactory;
        protected readonly Type HandlerType;
        private readonly Type _eventType;
        private readonly ILogger _logger;

        protected EventMessageDispatcher(IDependencyResolver dependencyResolver,
                                         IBrokeredMessageFactory brokeredMessageFactory,
                                         IInboundInterceptorFactory inboundInterceptorFactory,
                                         Type handlerType,
                                         IClock clock,
                                         Type eventType,
            ILogger logger)
        {
            _dependencyResolver = dependencyResolver;
            _brokeredMessageFactory = brokeredMessageFactory;
            _clock = clock;
            _inboundInterceptorFactory = inboundInterceptorFactory;
            _eventType = eventType;
            _logger = logger;
            HandlerType = handlerType;
        }

        public async Task Dispatch(BrokeredMessage message)
        {
            var busEvent = await _brokeredMessageFactory.GetBody(message, _eventType);
            await Dispatch((dynamic) busEvent, message);
        }

        protected abstract object CreateHandlerFromScope<TBusEvent>(IDependencyResolverScope scope, TBusEvent busEvent) where TBusEvent : IBusEvent;
        protected abstract Task DispatchToHandleMethod<TBusEvent>(TBusEvent busEvent, object handler) where TBusEvent : IBusEvent;

        private async Task Dispatch<TBusEvent>(TBusEvent busEvent, BrokeredMessage message) where TBusEvent : IBusEvent
        {
            using (var scope = _dependencyResolver.CreateChildScope())
            {
                var handler = CreateHandlerFromScope(scope, busEvent);
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
                    var wrapper = new LongLivedTaskWrapper(handlerTask, handler as ILongRunningTask, message, _clock);
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