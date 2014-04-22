using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;

namespace Nimbus.Interceptors.Outbound
{
    public interface IOutboundInterceptor
    {
        Task Decorate(BrokeredMessage brokeredMessage, object busMessage);
    }
}