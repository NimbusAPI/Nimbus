using System.Collections.Generic;
using System.Linq;
using Castle.MicroKernel;
using Castle.MicroKernel.Lifestyle;
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

        public OwnedComponent<IEnumerable<IHandleCompetingEvent<TBusEvent>>> GetHandler<TBusEvent>() where TBusEvent : IBusEvent
        {
            using (_container.BeginScope())
            {
                var type = typeof (IHandleCompetingEvent<TBusEvent>);
                var handlers = _container.ResolveAll(type).Cast<IHandleCompetingEvent<TBusEvent>>().ToArray();
                return new OwnedComponent<IEnumerable<IHandleCompetingEvent<TBusEvent>>>(handlers); //FIXME memory leak here.
            }
        }
    }
}