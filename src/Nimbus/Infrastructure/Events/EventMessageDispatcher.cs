using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Nimbus.DependencyResolution;
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

        protected EventMessageDispatcher(IDependencyResolver dependencyResolver,
                                         IBrokeredMessageFactory brokeredMessageFactory,
                                         IInboundInterceptorFactory inboundInterceptorFactory,
                                         Type handlerType,
                                         IClock clock,
                                         Type eventType)
        {
            _dependencyResolver = dependencyResolver;
            _brokeredMessageFactory = brokeredMessageFactory;
            _clock = clock;
            _inboundInterceptorFactory = inboundInterceptorFactory;
            _eventType = eventType;
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

                foreach (var interceptor in interceptors)
                {
                    await interceptor.OnEventHandlerExecuting(busEvent, message);
                }

                Exception exception;
                try
                {
                    var handlerTask = DispatchToHandleMethod(busEvent, handler);
                    var wrapper = new LongLivedTaskWrapper(handlerTask, handler as ILongRunningTask, message, _clock);
                    await wrapper.AwaitCompletion();

                    foreach (var interceptor in interceptors.Reverse())
                    {
                        await interceptor.OnEventHandlerSuccess(busEvent, message);
                    }
                    return;
                }
                catch (Exception exc)
                {
                    exception = exc;
                }

                foreach (var interceptor in interceptors.Reverse())
                {
                    await interceptor.OnEventHandlerError(busEvent, message, exception);
                }
                throw exception;
            }
        }
    }
}