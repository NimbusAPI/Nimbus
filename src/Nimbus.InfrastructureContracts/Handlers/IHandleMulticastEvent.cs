using System.Threading.Tasks;
using Nimbus.MessageContracts;

namespace Nimbus.InfrastructureContracts.Handlers
{
    public interface IHandleMulticastEvent<TBusEvent> where TBusEvent : IBusEvent
    {
        Task Handle(TBusEvent busEvent);
    }
}