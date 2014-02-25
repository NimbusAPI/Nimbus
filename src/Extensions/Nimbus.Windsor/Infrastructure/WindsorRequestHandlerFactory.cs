using Castle.MicroKernel;
using Castle.MicroKernel.Lifestyle;
using Nimbus.HandlerFactories;
using Nimbus.Handlers;
using Nimbus.MessageContracts;

namespace Nimbus.Windsor.Infrastructure
{
    public class WindsorRequestHandlerFactory : IRequestHandlerFactory
    {
        private readonly IKernel _container;

        public WindsorRequestHandlerFactory(IKernel container)
        {
            _container = container;
        }

        public OwnedComponent<IHandleRequest<TBusRequest, TBusResponse>> GetHandler<TBusRequest, TBusResponse>() where TBusRequest : IBusRequest<TBusRequest, TBusResponse>
            where TBusResponse : IBusResponse
        {
            var scope = _container.BeginScope();
            var handler = _container.Resolve<IHandleRequest<TBusRequest, TBusResponse>>();
            return new OwnedComponent<IHandleRequest<TBusRequest, TBusResponse>>(handler, scope);
        }
    }
}