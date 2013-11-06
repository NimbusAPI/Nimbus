using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Nimbus.InfrastructureContracts;
using Nimbus.MessageContracts;

namespace Nimbus.Autofac
{
    public class AutofacCompetingEventBroker: ICompetingEventBroker
    {
        private readonly ILifetimeScope _lifetimeScope;

        public AutofacCompetingEventBroker(ILifetimeScope lifetimeScope)
        {
            _lifetimeScope = lifetimeScope;
        }

        public void Publish<TBusEvent>(TBusEvent busEvent) where TBusEvent : IBusEvent
        {
            using (var scope = _lifetimeScope.BeginLifetimeScope())
            {
                var type = typeof(IEnumerable<IHandleMulticastEvent<TBusEvent>>);
                var handlers = (IEnumerable)scope.Resolve(type);
                foreach (var handler in handlers.Cast<IHandleMulticastEvent<IBusEvent>>()) handler.Handle(busEvent);
            }
        }
    }
}