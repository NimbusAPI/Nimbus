using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nimbus.MessageContracts;

namespace Nimbus.InfrastructureContracts
{
    public interface IMulticastRequestSender
    {
        Task<IEnumerable<TResponse>> SendRequest<TRequest, TResponse>(BusRequest<TRequest, TResponse> busRequest, TimeSpan timeout)
            where TRequest : IBusRequest<TRequest, TResponse>
            where TResponse : IBusResponse;
    }
}