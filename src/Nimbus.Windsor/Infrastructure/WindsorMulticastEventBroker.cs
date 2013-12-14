using System.Linq;
using Castle.MicroKernel;
using Castle.MicroKernel.Lifestyle;
using Nimbus.InfrastructureContracts;
using Nimbus.MessageContracts;

namespace Nimbus.Windsor.Infrastructure
{
    public class WindsorMulticastEventBroker : IMulticastEventBroker
    {
        private readonly IKernel _container;

        public WindsorMulticastEventBroker(IKernel container)
        {
            _container = container;
        }

        public void PublishMulticast<TBusEvent>(TBusEvent busEvent) where TBusEvent : IBusEvent
        {
            using (_container.BeginScope())
            {
                var type = typeof (IHandleMulticastEvent<TBusEvent>);
                var handlers = _container.ResolveAll(type).Cast<IHandleMulticastEvent<TBusEvent>>();
                foreach (var handler in handlers) handler.Handle(busEvent);
            }
        }
    }
}