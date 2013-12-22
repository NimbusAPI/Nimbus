using Autofac;
using Nimbus.InfrastructureContracts;
using Nimbus.MessageContracts;

namespace Nimbus.Autofac.Infrastructure
{
    public class AutofacRequestBroker : IRequestBroker
    {
        private readonly ILifetimeScope _lifetimeScope;

        public AutofacRequestBroker(ILifetimeScope lifetimeScope)
        {
            _lifetimeScope = lifetimeScope;
        }

        public TBusResponse Handle<TBusRequest, TBusResponse>(TBusRequest request)
            where TBusRequest : IBusRequest<TBusRequest, TBusResponse>
            where TBusResponse : IBusResponse
        {
            using (var scope = _lifetimeScope.BeginLifetimeScope())
            {
                var type = typeof (IHandleRequest<TBusRequest, TBusResponse>);

                var handler = (IHandleRequest<TBusRequest, TBusResponse>) scope.Resolve(type);
                var response = handler.Handle(request);

                return response;
            }
        }
    }
}