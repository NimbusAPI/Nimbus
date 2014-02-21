using System.Collections.Generic;
using Castle.MicroKernel;
using Nimbus.InfrastructureContracts;
using Nimbus.MessageContracts;

namespace Nimbus.Windsor.Infrastructure
{
    public class WindsorCompetingEventHandlerFactory : ICompetingEventHandlerFactory
    {
        private readonly IKernel _container;

        public WindsorCompetingEventHandlerFactory(IKernel container)
        {
            _container = container;
        }

        public OwnedComponent<IEnumerable<IHandleCompetingEvent<TBusEvent>>> GetHandlers<TBusEvent>() where TBusEvent : IBusEvent
        {
            var handlers = _container.ResolveAll<IHandleCompetingEvent<TBusEvent>>();
            return new OwnedComponent<IEnumerable<IHandleCompetingEvent<TBusEvent>>>(handlers); //FIXME memory leak here.
        }
    }
}