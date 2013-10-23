using System.Threading.Tasks;

namespace Nimbus.Infrastructure
{
    public interface IEventSender
    {
        Task Publish<TBusEvent>(TBusEvent busEvent);
    }
}