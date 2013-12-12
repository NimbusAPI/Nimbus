using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Castle.MicroKernel.Lifestyle;
using Castle.Windsor;
using Nimbus.Extensions;
using Nimbus.InfrastructureContracts;
using Nimbus.MessageContracts;
using Castle.MicroKernel;

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
            where TBusRequest : BusRequest<TBusRequest, TBusResponse>
            where TBusResponse : IBusResponse
        {
            using (var scope = _container.BeginScope())
            {
                var handlers = _container.Resolve<IEnumerable<IHandleRequest<TBusRequest, TBusResponse>>>();

                return handlers.Select(handler => Task.Run(() => handler.Handle(request)))
                               .ReturnOpportunistically(timeout);
            }
        }
    }
}