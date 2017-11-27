using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nimbus.MessageContracts;

namespace Nimbus.Infrastructure.RequestResponse
{
    internal interface IMulticastRequestSender
    {
        Task<IEnumerable<TResponse>> SendRequest<TRequest, TResponse>(IBusMulticastRequest<TRequest, TResponse> busRequest, TimeSpan timeout)
            where TRequest : IBusMulticastRequest<TRequest, TResponse>
            where TResponse : IBusMulticastResponse;
    }
}