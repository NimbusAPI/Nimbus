using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;

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
                var type = typeof(IHandleEvent<TBusEvent>);

                var handler = (IHandleEvent<IBusEvent>)scope.Resolve(type);
                handler.Handle(busEvent);
            }
        }
    }
}
