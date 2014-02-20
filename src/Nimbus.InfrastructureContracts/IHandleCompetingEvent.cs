using System.Threading.Tasks;
using Nimbus.MessageContracts;

namespace Nimbus.InfrastructureContracts
{
    public interface IHandleCompetingEvent<TBusEvent> where TBusEvent : IBusEvent
    {
        Task Handle(TBusEvent busEvent);
    }
}