using System.Threading.Tasks;

// ReSharper disable CheckNamespace

namespace Nimbus
// ReSharper restore CheckNamespace
{
    public interface IDeadLetterQueue
    {
        Task<TBusMessageContract> Pop<TBusMessageContract>() where TBusMessageContract : class;
    }
}