using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Castle.MicroKernel;
using Castle.MicroKernel.Lifestyle;
using Nimbus.InfrastructureContracts;
using Nimbus.MessageContracts;

namespace Nimbus.Windsor.Infrastructure
{
    public class WindsorCompetingEventBroker : ICompetingEventBroker
    {
        private readonly IKernel _container;

        public WindsorCompetingEventBroker(IKernel container)
        {
            _container = container;
        }

        public void PublishCompeting<TBusEvent>(TBusEvent busEvent) where TBusEvent : IBusEvent
        {
            using (_container.BeginScope())
            {
                var type = typeof (IHandleCompetingEvent<TBusEvent>);
                var handlers = _container.ResolveAll(type).Cast<IHandleCompetingEvent<TBusEvent>>();
                foreach (var handler in handlers) handler.Handle(busEvent);
            }
        }
    }
}