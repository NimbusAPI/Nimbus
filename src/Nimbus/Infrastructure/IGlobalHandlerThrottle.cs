using System.Threading;
using System.Threading.Tasks;

namespace Nimbus.Infrastructure
{
    internal interface IGlobalHandlerThrottle
    {
        Task Wait(CancellationToken cancellationToken);
        void Release();
    }
}