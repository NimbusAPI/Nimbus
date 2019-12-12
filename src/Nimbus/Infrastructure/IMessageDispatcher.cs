using System.Threading.Tasks;
using Nimbus.InfrastructureContracts;

namespace Nimbus.Infrastructure
{
    internal interface IMessageDispatcher
    {
        Task Dispatch(NimbusMessage message);
    }
}