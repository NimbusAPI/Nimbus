using System;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;

namespace Nimbus.Interceptors
{
    public interface IMessageInterceptor<in TBusMessage>
    {
        int Priority { get; }

        Task OnHandlerExecuting(TBusMessage busEvent, BrokeredMessage brokeredMessage);
        Task OnHandlerSuccess(TBusMessage busEvent, BrokeredMessage brokeredMessage);
        Task OnHandlerError(TBusMessage busEvent, BrokeredMessage brokeredMessage, Exception exception);
    }
}