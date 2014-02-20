using Castle.MicroKernel;
using Castle.MicroKernel.Lifestyle;
using Nimbus.InfrastructureContracts;
using Nimbus.MessageContracts;

namespace Nimbus.Windsor.Infrastructure
{
    public class WindsorRequestBroker : IRequestBroker
    {
        private readonly IKernel _container;

        public WindsorRequestBroker(IKernel container)
        {
            _container = container;
        }

        public TBusResponse Handle<TBusRequest, TBusResponse>(TBusRequest request) where TBusRequest : IBusRequest<TBusRequest, TBusResponse> where TBusResponse : IBusResponse
        {
            using (_container.BeginScope())
            {
                var type = typeof (IHandleRequest<TBusRequest, TBusResponse>);

                var handler = (IHandleRequest<TBusRequest, TBusResponse>) _container.Resolve(type);
                var response = handler.Handle(request);

                return response.Result;
            }
        }
    }
}