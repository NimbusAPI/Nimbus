using Autofac;
using Autofac.Features.OwnedInstances;
using Nimbus.InfrastructureContracts;
using Nimbus.MessageContracts;

namespace Nimbus.Autofac.Infrastructure
{
    public class AutofacRequestHandlerFactory : IRequestHandlerFactory
    {
        private readonly ILifetimeScope _lifetimeScope;

        public AutofacRequestHandlerFactory(ILifetimeScope lifetimeScope)
        {
            _lifetimeScope = lifetimeScope;
        }

        public OwnedComponent<IHandleRequest<TBusRequest, TBusResponse>> GetHandler<TBusRequest, TBusResponse>() where TBusRequest : IBusRequest<TBusRequest, TBusResponse>
            where TBusResponse : IBusResponse
        {
            var owned = _lifetimeScope.Resolve<Owned<IHandleRequest<TBusRequest, TBusResponse>>>();
            return new OwnedComponent<IHandleRequest<TBusRequest, TBusResponse>>(owned.Value, owned);
        }
    }
}