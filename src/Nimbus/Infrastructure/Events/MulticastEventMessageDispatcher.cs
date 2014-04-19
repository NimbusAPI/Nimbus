using System;
using System.Threading.Tasks;
using Nimbus.DependencyResolution;
using Nimbus.Handlers;

namespace Nimbus.Infrastructure.Events
{
    internal class MulticastEventMessageDispatcher : EventMessageDispatcher
    {
        public MulticastEventMessageDispatcher(IDependencyResolver dependencyResolver, IBrokeredMessageFactory brokeredMessageFactory, Type handlerType, IClock clock, Type eventType)
            : base(dependencyResolver, brokeredMessageFactory, handlerType, clock, eventType)
        {
        }

        protected override void CreateHandlerTaskFromScope<TBusEvent>(TBusEvent busEvent,
                                                                      IDependencyResolverScope scope,
                                                                      out Task handlerTask,
                                                                      out ILongRunningTask longRunningHandler)
        {
            var handler = scope.Resolve<IHandleMulticastEvent<TBusEvent>>(HandlerType.FullName);
            handlerTask = handler.Handle(busEvent);
            longRunningHandler = handler as ILongRunningTask;
        }
    }
}