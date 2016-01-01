using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nimbus.DependencyResolution;
using Nimbus.Handlers;
using Nimbus.Infrastructure.PropertyInjection;
using Nimbus.Interceptors.Inbound;

namespace Nimbus.Infrastructure.Events
{
    internal class MulticastEventMessageDispatcher : EventMessageDispatcher
    {
        private readonly IPropertyInjector _propertyInjector;

        public MulticastEventMessageDispatcher(IDependencyResolver dependencyResolver,
                                               IInboundInterceptorFactory inboundInterceptorFactory,
                                               IReadOnlyDictionary<Type, Type[]> handlerMap,
                                               IPropertyInjector propertyInjector,
                                               ILogger logger)
            : base(dependencyResolver, handlerMap, inboundInterceptorFactory, logger)
        {
            _propertyInjector = propertyInjector;
        }

        protected override object CreateHandlerFromScope<TBusEvent>(IDependencyResolverScope scope, TBusEvent busEvent, Type handlerType, NimbusMessage nimbusMessage)
        {
            var handler = (IHandleMulticastEvent<TBusEvent>) scope.Resolve(handlerType);
            _propertyInjector.Inject(handler, nimbusMessage);
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