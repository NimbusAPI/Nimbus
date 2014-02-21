using System.Collections.Generic;
using Castle.MicroKernel;
using Nimbus.InfrastructureContracts;
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
            var handlers = _container.ResolveAll<IHandleMulticastEvent<TBusEvent>>();
            return new OwnedComponent<IEnumerable<IHandleMulticastEvent<TBusEvent>>>(handlers); //FIXME memory leak!
        }
    }
}