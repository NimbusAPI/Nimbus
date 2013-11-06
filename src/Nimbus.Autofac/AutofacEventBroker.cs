using System.Collections.Generic;
using Autofac;
using Nimbus.InfrastructureContracts;
using Nimbus.MessageContracts;

namespace Nimbus.Autofac
{
    public class AutofacEventBroker : IEventBroker
    {
        private readonly ILifetimeScope _lifetimeScope;

        public AutofacEventBroker(ILifetimeScope lifetimeScope)
        {
            _lifetimeScope = lifetimeScope;
        }

        public void Publish<TBusEvent>(TBusEvent busEvent) where TBusEvent : IBusEvent
        {
            using (var scope = _lifetimeScope.BeginLifetimeScope())
            {
                var handlers = scope.Resolve<IEnumerable<IHandleEvent<TBusEvent>>>();
                foreach (var handler in handlers)
                {
                    handler.Handle(busEvent);
                }
            }
        }
    }
}
