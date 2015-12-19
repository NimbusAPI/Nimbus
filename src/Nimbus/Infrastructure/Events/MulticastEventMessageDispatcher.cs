using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nimbus.Configuration.Settings;
using Nimbus.DependencyResolution;
using Nimbus.Handlers;
using Nimbus.Infrastructure.PropertyInjection;
using Nimbus.Interceptors.Inbound;

namespace Nimbus.Infrastructure.Events
{
    internal class MulticastEventMessageDispatcher : EventMessageDispatcher
    {
        private readonly IPropertyInjector _propertyInjector;

        public MulticastEventMessageDispatcher(INimbusMessageFactory nimbusMessageFactory,
                                               IClock clock,
                                               IDependencyResolver dependencyResolver,
                                               IInboundInterceptorFactory inboundInterceptorFactory,
                                               IReadOnlyDictionary<Type, Type[]> handlerMap,
                                               DefaultMessageLockDurationSetting defaultMessageLockDuration,
                                               IPropertyInjector propertyInjector,
                                               ILogger logger)
            : base(dependencyResolver, handlerMap, inboundInterceptorFactory, logger)
        {
            _propertyInjector = propertyInjector;
        }

        protected override object CreateHandlerFromScope<TBusEvent>(IDependencyResolverScope scope, TBusEvent busEvent, Type handlerType, NimbusMessage brokeredMessage)
        {
            var handler = (IHandleMulticastEvent<TBusEvent>) scope.Resolve(handlerType);
            _propertyInjector.Inject(handler, brokeredMessage);
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