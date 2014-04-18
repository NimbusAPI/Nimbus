using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Nimbus.DependencyResolution;
using Nimbus.Handlers;
using Nimbus.MessageContracts;

namespace Nimbus.Infrastructure.Events
{
    internal abstract class EventMessageDispather : IMessageDispatcher
    {
        private readonly IDependencyResolver _dependencyResolver;
        private readonly IBrokeredMessageFactory _brokeredMessageFactory;
        private readonly IClock _clock;
        protected readonly Type HandlerType;

        protected EventMessageDispather(IDependencyResolver dependencyResolver, IBrokeredMessageFactory brokeredMessageFactory, Type handlerType, IClock clock)
        {
            _dependencyResolver = dependencyResolver;
            _brokeredMessageFactory = brokeredMessageFactory;
            _clock = clock;
            HandlerType = handlerType;
        }

        public async Task Dispatch(BrokeredMessage message)
        {
            var busEvent = await _brokeredMessageFactory.GetBody(message, HandlerType);
            await Dispatch((dynamic) busEvent, message);
        }

        private async Task Dispatch<TBusEvent>(TBusEvent busEvent, BrokeredMessage message) where TBusEvent : IBusEvent
        {
            using (var scope = _dependencyResolver.CreateChildScope())
            {
                var wrapperTasks = new List<Task>();

                ILongRunningHandler longRunningHandler;
                Task handlerTask;
                CreateHandlerTaskFromScope(busEvent, scope, out handlerTask, out longRunningHandler);

                var wrapperTask = new LongLivedTaskWrapper(handlerTask, longRunningHandler, message, _clock);
                wrapperTasks.Add(wrapperTask.AwaitCompletion());

                await Task.WhenAll(wrapperTasks);
            }
        }

        protected abstract void CreateHandlerTaskFromScope<TBusEvent>(TBusEvent busEvent,
                                                                      IDependencyResolverScope scope,
                                                                      out Task handlerTask,
                                                                      out ILongRunningHandler longRunningHandler) where TBusEvent : IBusEvent;
    }
}