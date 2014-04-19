using System;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Nimbus.DependencyResolution;
using Nimbus.Handlers;
using Nimbus.MessageContracts;

namespace Nimbus.Infrastructure.Events
{
    internal abstract class EventMessageDispatcher : IMessageDispatcher
    {
        private readonly IDependencyResolver _dependencyResolver;
        private readonly IBrokeredMessageFactory _brokeredMessageFactory;
        private readonly IClock _clock;
        protected readonly Type HandlerType;
        private readonly Type _eventType;

        protected EventMessageDispatcher(IDependencyResolver dependencyResolver, IBrokeredMessageFactory brokeredMessageFactory, Type handlerType, IClock clock, Type eventType)
        {
            _dependencyResolver = dependencyResolver;
            _brokeredMessageFactory = brokeredMessageFactory;
            _clock = clock;
            _eventType = eventType;
            HandlerType = handlerType;
        }

        public async Task Dispatch(BrokeredMessage message)
        {
            var busEvent = await _brokeredMessageFactory.GetBody(message, _eventType);
            await Dispatch((dynamic) busEvent, message);
        }

        private async Task Dispatch<TBusEvent>(TBusEvent busEvent, BrokeredMessage message) where TBusEvent : IBusEvent
        {
            using (var scope = _dependencyResolver.CreateChildScope())
            {
                Task handlerTask;
                ILongRunningTask longRunningHandler;
                CreateHandlerTaskFromScope(busEvent, scope, out handlerTask, out longRunningHandler);

                var wrapperTask = new LongLivedTaskWrapper(handlerTask, longRunningHandler, message, _clock);
                await wrapperTask.AwaitCompletion();
            }
        }

        protected abstract void CreateHandlerTaskFromScope<TBusEvent>(TBusEvent busEvent,
                                                                      IDependencyResolverScope scope,
                                                                      out Task handlerTask,
                                                                      out ILongRunningTask longRunningHandler) where TBusEvent : IBusEvent;
    }
}