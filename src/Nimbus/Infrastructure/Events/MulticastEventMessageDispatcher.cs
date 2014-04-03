using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Nimbus.Extensions;
using Nimbus.HandlerFactories;
using Nimbus.Handlers;
using Nimbus.MessageContracts;

namespace Nimbus.Infrastructure.Events
{
    internal class MulticastEventMessageDispatcher : IMessageDispatcher
    {
        private readonly IMulticastEventHandlerFactory _multicastEventHandlerFactory;
        private readonly IBrokeredMessageFactory _brokeredMessageFactory;
        private readonly Type _eventType;
        private readonly IClock _clock;

        public MulticastEventMessageDispatcher(
            IMulticastEventHandlerFactory multicastEventHandlerFactory, 
            IBrokeredMessageFactory brokeredMessageFactory,
            Type eventType, 
            IClock clock)
        {
            _multicastEventHandlerFactory = multicastEventHandlerFactory;
            _brokeredMessageFactory = brokeredMessageFactory;
            _eventType = eventType;
            _clock = clock;
        }

        public async Task Dispatch(BrokeredMessage message)
        {
            var busEvent = _brokeredMessageFactory.GetBody(message, _eventType);
            await Dispatch((dynamic) busEvent, message);
        }

        private async Task Dispatch<TBusEvent>(TBusEvent busEvent, BrokeredMessage message) where TBusEvent : IBusEvent
        {
            using (var handlers = _multicastEventHandlerFactory.GetHandlers<TBusEvent>())
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