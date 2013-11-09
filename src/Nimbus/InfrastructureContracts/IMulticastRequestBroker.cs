using System;
using System.Collections.Generic;
using Nimbus.MessageContracts;

namespace Nimbus.InfrastructureContracts
{
    public interface IMulticastRequestBroker
    {
        IEnumerable<TBusResponse> HandleMulticast<TBusRequest, TBusResponse>(TBusRequest request, TimeSpan timeout)
            where TBusRequest : BusRequest<TBusRequest, TBusResponse>
            where TBusResponse : IBusResponse;
    }
}