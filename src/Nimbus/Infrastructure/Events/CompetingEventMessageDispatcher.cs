using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Nimbus.HandlerFactories;
using Nimbus.Handlers;
using Nimbus.MessageContracts;

namespace Nimbus.Infrastructure.Events
{
    internal class CompetingEventMessageDispatcher : IMessageDispatcher
    {
        private readonly ICompetingEventHandlerFactory _competingEventHandlerFactory;
        private readonly IBrokeredMessageFactory _brokeredMessageFactory;
        private readonly Type _eventType;
        private readonly IClock _clock;

        public CompetingEventMessageDispatcher(
            ICompetingEventHandlerFactory competingEventHandlerFactory, 
            IBrokeredMessageFactory brokeredMessageFactory, 
            Type eventType, IClock clock)
        {
            _competingEventHandlerFactory = competingEventHandlerFactory;
            _brokeredMessageFactory = brokeredMessageFactory;
            _eventType = eventType;
            _clock = clock;
        }

        public async Task Dispatch(BrokeredMessage message)
        {
            var busEvent = await _brokeredMessageFactory.GetBody(message, _eventType);
            await Dispatch((dynamic) busEvent, message);
        }

        private async Task Dispatch<TBusEvent>(TBusEvent busEvent, BrokeredMessage message) where TBusEvent : IBusEvent
        {
            using (var handlers = _competingEventHandlerFactory.GetHandlers<TBusEvent>())
            {
                var wrapperTasks = new List<Task>();

                foreach (var handler in handlers.Component)
                {
                    var handlerTask = handler.Handle(busEvent);
                    var wrapperTask = new LongLivedTaskWrapper(handlerTask, handler as ILongRunningHandler, message, _clock);
                    wrapperTasks.Add(wrapperTask.AwaitCompletion());
                }

                await Task.WhenAll(wrapperTasks);
            }
        }
    }
}