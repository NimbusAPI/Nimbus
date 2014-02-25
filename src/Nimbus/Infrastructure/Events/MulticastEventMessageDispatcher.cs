using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Nimbus.Extensions;
using Nimbus.HandlerFactories;
using Nimbus.MessageContracts;

namespace Nimbus.Infrastructure.Events
{
    public class MulticastEventMessageDispatcher : IMessageDispatcher
    {
        private readonly IMulticastEventHandlerFactory _multicastEventHandlerFactory;
        private readonly Type _eventType;

        public MulticastEventMessageDispatcher(IMulticastEventHandlerFactory multicastEventHandlerFactory, Type eventType)
        {
            _multicastEventHandlerFactory = multicastEventHandlerFactory;
            _eventType = eventType;
        }

        public async Task Dispatch(BrokeredMessage message)
        {
            var busEvent = message.GetBody(_eventType);
            await Dispatch((dynamic) busEvent, message);
        }

        private async Task Dispatch<TBusEvent>(TBusEvent busEvent, BrokeredMessage message) where TBusEvent : IBusEvent
        {
            using (var handlers = _multicastEventHandlerFactory.GetHandlers<TBusEvent>())
            {
                await Task.WhenAll(handlers.Component.Select(h => h.Handle(busEvent)));
            }
        }
    }
}