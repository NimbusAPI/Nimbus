using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Nimbus.Extensions;
using Nimbus.InfrastructureContracts;
using Nimbus.MessageContracts;

namespace Nimbus.Autofac.Infrastructure
{
    public class AutofacMulticastRequestBroker : IMulticastRequestBroker
    {
        private readonly ILifetimeScope _lifetimeScope;

        public AutofacMulticastRequestBroker(ILifetimeScope lifetimeScope)
        {
            _lifetimeScope = lifetimeScope;
        }

        public IEnumerable<TBusResponse> HandleMulticast<TBusRequest, TBusResponse>(TBusRequest request, TimeSpan timeout)
            where TBusRequest : BusRequest<TBusRequest, TBusResponse>
            where TBusResponse : IBusResponse
        {
            using (var scope = _lifetimeScope.BeginLifetimeScope())
            {
                var handlers = scope.Resolve<IEnumerable<IHandleRequest<TBusRequest, TBusResponse>>>();

                return handlers.Select(handler => Task.Run(() => handler.Handle(request)))
                               .ReturnOpportunistically(timeout);
            }
        }
    }
}