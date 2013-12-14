using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Castle.MicroKernel;
using Castle.MicroKernel.Lifestyle;
using Nimbus.Extensions;
using Nimbus.InfrastructureContracts;
using Nimbus.MessageContracts;

namespace Nimbus.Windsor.Infrastructure
{
    public class WindsorMulticastRequestBroker : IMulticastRequestBroker
    {
        private readonly IKernel _container;

        public WindsorMulticastRequestBroker(IKernel container)
        {
            _container = container;
        }

        public IEnumerable<TBusResponse> HandleMulticast<TBusRequest, TBusResponse>(TBusRequest request, TimeSpan timeout)
            where TBusRequest : IBusRequest<TBusRequest, TBusResponse>
            where TBusResponse : IBusResponse
        {
            using (_container.BeginScope())
            {
                var handlers = _container.Resolve<IEnumerable<IHandleRequest<TBusRequest, TBusResponse>>>();

                return handlers.Select(handler => Task.Run(() => handler.Handle(request)))
                               .ReturnOpportunistically(timeout);
            }
        }
    }
}