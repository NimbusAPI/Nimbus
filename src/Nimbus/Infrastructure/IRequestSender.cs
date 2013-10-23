using System.Threading.Tasks;
using Nimbus.MessageContracts;

namespace Nimbus.Infrastructure
{
    public interface IRequestSender
    {
        Task<TResponse> SendRequest<TRequest, TResponse>(BusRequest<TRequest, TResponse> busRequest);
    }
}