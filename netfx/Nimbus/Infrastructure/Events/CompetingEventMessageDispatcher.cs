﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nimbus.DependencyResolution;
using Nimbus.Handlers;
using Nimbus.Infrastructure.Filtering;
using Nimbus.Infrastructure.PropertyInjection;
using Nimbus.Interceptors.Inbound;

namespace Nimbus.Infrastructure.Events
{
    internal class CompetingEventMessageDispatcher : EventMessageDispatcher
    {
        private readonly IPropertyInjector _propertyInjector;

        public CompetingEventMessageDispatcher(IDependencyResolver dependencyResolver,
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
            var handler = (IHandleCompetingEvent<TBusEvent>) scope.Resolve(handlerType);
            _propertyInjector.Inject(handler, nimbusMessage);
            return handler;
        }

        protected override Task DispatchToHandleMethod<TBusEvent>(TBusEvent busEvent, object handler)
        {
            var genericHandler = (IHandleCompetingEvent<TBusEvent>) handler;
            var handlerTask = genericHandler.Handle(busEvent);
            return handlerTask;
        }
    }
}