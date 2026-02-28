using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nimbus.Infrastructure.Filtering;
using Nimbus.Infrastructure.PropertyInjection;
using Nimbus.InfrastructureContracts;
using Nimbus.InfrastructureContracts.DependencyResolution;
using Nimbus.InfrastructureContracts.Handlers;
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
                                               ILogger logger,
                                               IFilterConditionProvider filterConditionProvider)
            : base(dependencyResolver, handlerMap, inboundInterceptorFactory, filterConditionProvider, logger)
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