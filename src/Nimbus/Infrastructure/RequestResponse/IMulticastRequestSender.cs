using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nimbus.MessageContracts;

namespace Nimbus.Infrastructure.RequestResponse
{
    internal interface IMulticastRequestSender
    {
        Task<IEnumerable<TResponse>> SendRequest<TRequest, TResponse>(IBusRequest<TRequest, TResponse> busRequest, TimeSpan timeout)
            where TRequest : IBusRequest<TRequest, TResponse>
            where TResponse : IBusResponse;
    }
}