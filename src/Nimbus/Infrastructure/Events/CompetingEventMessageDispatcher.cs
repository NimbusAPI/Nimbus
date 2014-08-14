using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nimbus.Configuration.Settings;
using Nimbus.DependencyResolution;
using Nimbus.Exceptions;
using Nimbus.Extensions;
using Nimbus.Handlers;
using Nimbus.Interceptors.Inbound;
using Nimbus.Logger;

namespace Nimbus.Infrastructure.Events
{
    internal class CompetingEventMessageDispatcher : EventMessageDispatcher
    {
        public CompetingEventMessageDispatcher(IBrokeredMessageFactory brokeredMessageFactory, IClock clock, IDependencyResolver dependencyResolver, IInboundInterceptorFactory inboundInterceptorFactory, IReadOnlyDictionary<Type, Type[]> handlerMap, DefaultMessageLockDurationSetting defaultMessageLockDuration)
            : base(brokeredMessageFactory, clock, dependencyResolver, handlerMap, inboundInterceptorFactory, new NullLogger(), defaultMessageLockDuration)
        {
        }

        protected override object CreateHandlerFromScope<TBusEvent>(IDependencyResolverScope scope, TBusEvent busEvent, Type handlerType)
        {
            var handler = scope.Resolve<IHandleCompetingEvent<TBusEvent>>(handlerType.FullName);
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