using System.Threading.Tasks;

namespace Nimbus
{
    public interface IEventSender
    {
        Task Publish<TBusEvent>(TBusEvent busEvent);
    }
}