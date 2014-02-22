using System.Collections.Generic;
using Castle.MicroKernel;
using Castle.MicroKernel.Lifestyle;
using Nimbus.HandlerFactories;
using Nimbus.Handlers;
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
            var scope = _container.BeginScope();
            var handlers = _container.ResolveAll<IHandleCompetingEvent<TBusEvent>>();
            return new OwnedComponent<IEnumerable<IHandleCompetingEvent<TBusEvent>>>(handlers, scope);
        }
    }
}