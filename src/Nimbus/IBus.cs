using System.Threading.Tasks;
using Nimbus.MessageContracts;

namespace Nimbus
{
    public interface IBus
    {
        Task Send<TBusCommand>(TBusCommand busCommand);
        Task<TResponse> Request<TRequest, TResponse>(BusRequest<TRequest, TResponse> busRequest);
        Task Publish<TBusEvent>(TBusEvent busEvent);
    }
}