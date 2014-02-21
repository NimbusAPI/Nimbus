using System.Collections.Generic;
using Autofac;
using Autofac.Features.OwnedInstances;
using Nimbus.InfrastructureContracts;
using Nimbus.MessageContracts;

namespace Nimbus.Autofac.Infrastructure
{
    public class AutofacMulticastEventHandlerFactory : IMulticastEventHandlerFactory
    {
        private readonly ILifetimeScope _lifetimeScope;

        public AutofacMulticastEventHandlerFactory(ILifetimeScope lifetimeScope)
        {
            _lifetimeScope = lifetimeScope;
        }

        public OwnedComponent<IEnumerable<IHandleMulticastEvent<TBusEvent>>> GetHandlers<TBusEvent>() where TBusEvent : IBusEvent
        {
            var owned = _lifetimeScope.Resolve<Owned<IEnumerable<IHandleMulticastEvent<TBusEvent>>>>();
            return new OwnedComponent<IEnumerable<IHandleMulticastEvent<TBusEvent>>>(owned.Value, owned);
        }
    }
}