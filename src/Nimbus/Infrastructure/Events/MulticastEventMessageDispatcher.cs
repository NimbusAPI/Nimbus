using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nimbus.DependencyResolution;
using Nimbus.Exceptions;
using Nimbus.Extensions;
using Nimbus.Handlers;
using Nimbus.Interceptors.Inbound;
using Nimbus.Logger;

namespace Nimbus.Infrastructure.Events
{
    internal class MulticastEventMessageDispatcher : EventMessageDispatcher
    {
        private readonly IReadOnlyDictionary<Type, Type> _handlerMap;

        public MulticastEventMessageDispatcher(IBrokeredMessageFactory brokeredMessageFactory,
                                               IClock clock,
                                               IDependencyResolver dependencyResolver,
                                               IInboundInterceptorFactory inboundInterceptorFactory,
                                               IReadOnlyDictionary<Type, Type> handlerMap)
            : base(brokeredMessageFactory, clock, dependencyResolver, inboundInterceptorFactory, new NullLogger())
        {
            _handlerMap = handlerMap;
        }

        protected override object CreateHandlerFromScope<TBusEvent>(IDependencyResolverScope scope, TBusEvent busEvent)
        {
            Type handlerType;
            if (!_handlerMap.TryGetValue(busEvent.GetType(), out handlerType))
                throw new DispatchFailedException("There is no handler registered with this dispatcher for the message type {0}.".FormatWith(busEvent.GetType()));

            var handler = scope.Resolve<IHandleMulticastEvent<TBusEvent>>(handlerType.FullName);
            return handler;
        }

        protected override Task DispatchToHandleMethod<TBusEvent>(TBusEvent busEvent, object handler)
        {
            var genericHandler = (IHandleMulticastEvent<TBusEvent>) handler;
            var handlerTask = genericHandler.Handle(busEvent);
            return handlerTask;
        }
    }
}