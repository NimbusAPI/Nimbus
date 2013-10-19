using System.Threading.Tasks;

namespace Nimbus
{
    public interface IRequestSender
    {
        Task<TResponse> SendRequest<TRequest, TResponse>(BusRequest<TRequest, TResponse> busRequest);
    }
}