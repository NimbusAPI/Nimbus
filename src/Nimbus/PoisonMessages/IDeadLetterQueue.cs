using System.Threading.Tasks;

namespace Nimbus.PoisonMessages
{
    public interface IDeadLetterQueue
    {
        Task<TBusMessageContract> Pop<TBusMessageContract>() where TBusMessageContract : class;
    }
}