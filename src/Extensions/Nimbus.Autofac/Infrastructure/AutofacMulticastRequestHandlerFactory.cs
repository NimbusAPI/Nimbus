using System.Collections.Generic;
using Autofac;
using Autofac.Features.OwnedInstances;
using Nimbus.InfrastructureContracts;
using Nimbus.MessageContracts;

namespace Nimbus.Autofac.Infrastructure
{
    public class AutofacMulticastRequestHandlerFactory : IMulticastRequestHandlerFactory
    {
        private readonly ILifetimeScope _lifetimeScope;

        public AutofacMulticastRequestHandlerFactory(ILifetimeScope lifetimeScope)
        {
            _lifetimeScope = lifetimeScope;
        }

        public OwnedComponent<IEnumerable<IHandleRequest<TBusRequest, TBusResponse>>> GetHandlers<TBusRequest, TBusResponse>()
            where TBusRequest : IBusRequest<TBusRequest, TBusResponse>
            where TBusResponse : IBusResponse
        {
            var owned = _lifetimeScope.Resolve<Owned<IEnumerable<IHandleRequest<TBusRequest, TBusResponse>>>>();
            return new OwnedComponent<IEnumerable<IHandleRequest<TBusRequest, TBusResponse>>>(owned.Value, owned);
        }
    }
}