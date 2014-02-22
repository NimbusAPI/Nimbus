using System.Collections.Generic;
using Autofac;
using Autofac.Features.OwnedInstances;
using Nimbus.HandlerFactories;
using Nimbus.Handlers;
using Nimbus.MessageContracts;

namespace Nimbus.Autofac.Infrastructure
{
    public class AutofacCompetingEventHandlerFactory : ICompetingEventHandlerFactory
    {
        private readonly ILifetimeScope _lifetimeScope;

        public AutofacCompetingEventHandlerFactory(ILifetimeScope lifetimeScope)
        {
            _lifetimeScope = lifetimeScope;
        }

        public OwnedComponent<IEnumerable<IHandleCompetingEvent<TBusEvent>>> GetHandlers<TBusEvent>() where TBusEvent : IBusEvent
        {
            var owned = _lifetimeScope.Resolve<Owned<IEnumerable<IHandleCompetingEvent<TBusEvent>>>>();
            return new OwnedComponent<IEnumerable<IHandleCompetingEvent<TBusEvent>>>(owned.Value, owned);
        }
    }
}