using System.Collections.Generic;
using Castle.MicroKernel;
using Castle.MicroKernel.Lifestyle;
using Nimbus.HandlerFactories;
using Nimbus.Handlers;
using Nimbus.MessageContracts;

namespace Nimbus.Windsor.Infrastructure
{
    public class WindsorMulticastEventHandlerFactory : IMulticastEventHandlerFactory
    {
        private readonly IKernel _container;

        public WindsorMulticastEventHandlerFactory(IKernel container)
        {
            _container = container;
        }

        public OwnedComponent<IEnumerable<IHandleMulticastEvent<TBusEvent>>> GetHandlers<TBusEvent>() where TBusEvent : IBusEvent
        {
            var scope = _container.BeginScope();
            var handlers = _container.ResolveAll<IHandleMulticastEvent<TBusEvent>>();
            return new OwnedComponent<IEnumerable<IHandleMulticastEvent<TBusEvent>>>(handlers, scope);
        }
    }
}