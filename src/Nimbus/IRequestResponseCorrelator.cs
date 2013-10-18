using System.Threading.Tasks;

namespace Nimbus
{
    public interface IRequestResponseCorrelator
    {
        Task<TResponse> MakeCorrelatedRequest<TRequest, TResponse>(BusRequest<TRequest, TResponse> busRequest);
        void Start();
    }
}