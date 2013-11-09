using System.Threading.Tasks;

namespace Nimbus.Infrastructure.Events
{
    internal interface IEventSender
    {
        Task Publish<TBusEvent>(TBusEvent busEvent);
    }
}