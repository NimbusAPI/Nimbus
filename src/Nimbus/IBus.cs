using System.Threading.Tasks;
using Nimbus.MessageContracts;
using Nimbus.PoisonMessages;

namespace Nimbus
{
    public interface IBus
    {
        Task Send<TBusCommand>(TBusCommand busCommand);
        Task<TResponse> Request<TRequest, TResponse>(BusRequest<TRequest, TResponse> busRequest);
        Task Publish<TBusEvent>(TBusEvent busEvent);

        IDeadLetterQueues DeadLetterQueues { get; }
    }
}