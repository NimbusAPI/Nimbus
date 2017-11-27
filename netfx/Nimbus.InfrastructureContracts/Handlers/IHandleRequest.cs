﻿using System.Threading.Tasks;
using Nimbus.MessageContracts;

namespace Nimbus.Handlers
{
    public interface IHandleRequest<TBusRequest, TBusResponse>
        where TBusRequest : IBusRequest<TBusRequest, TBusResponse>
        where TBusResponse : IBusResponse
    {
        Task<TBusResponse> Handle(TBusRequest request);
    }
}