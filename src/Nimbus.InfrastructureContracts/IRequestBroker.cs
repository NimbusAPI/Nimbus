using System;
using Nimbus.MessageContracts;

namespace Nimbus.InfrastructureContracts
{
    [Obsolete("pending refactor")]
    public interface IRequestBroker
    {
        TBusResponse Handle<TBusRequest, TBusResponse>(TBusRequest request) where TBusRequest : IBusRequest<TBusRequest, TBusResponse> where TBusResponse : IBusResponse;
    }
}