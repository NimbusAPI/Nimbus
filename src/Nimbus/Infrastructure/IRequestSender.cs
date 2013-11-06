using System;
using System.Threading.Tasks;
using Nimbus.MessageContracts;

namespace Nimbus.Infrastructure
{
    public interface IRequestSender
    {
        Task<TResponse> SendRequest<TRequest, TResponse>(BusRequest<TRequest, TResponse> busRequest)
            where TRequest : IBusRequest
            where TResponse : IBusResponse;

        Task<TResponse> SendRequest<TRequest, TResponse>(BusRequest<TRequest, TResponse> busRequest, TimeSpan timeout)
            where TRequest : IBusRequest
            where TResponse : IBusResponse;
    }
}