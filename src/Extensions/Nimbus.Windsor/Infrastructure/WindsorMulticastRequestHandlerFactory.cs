using System.Collections.Generic;
using Castle.MicroKernel;
using Castle.MicroKernel.Lifestyle;
using Nimbus.HandlerFactories;
using Nimbus.Handlers;
using Nimbus.MessageContracts;

namespace Nimbus.Windsor.Infrastructure
{
    public class WindsorMulticastRequestHandlerFactory : IMulticastRequestHandlerFactory
    {
        private readonly IKernel _container;

        public WindsorMulticastRequestHandlerFactory(IKernel container)
        {
            _container = container;
        }

        public OwnedComponent<IEnumerable<IHandleRequest<TBusRequest, TBusResponse>>> GetHandlers<TBusRequest, TBusResponse>()
            where TBusRequest : IBusRequest<TBusRequest, TBusResponse> where TBusResponse : IBusResponse
        {
            var scope = _container.BeginScope();
            var handlers = _container.ResolveAll<IHandleRequest<TBusRequest, TBusResponse>>();
            return new OwnedComponent<IEnumerable<IHandleRequest<TBusRequest, TBusResponse>>>(handlers, scope);
        }
    }
}