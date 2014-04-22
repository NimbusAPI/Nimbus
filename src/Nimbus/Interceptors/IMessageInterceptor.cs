using System;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Nimbus.MessageContracts;

namespace Nimbus.Interceptors
{
    public interface IMessageInterceptor
    {
        int Priority { get; }

        Task OnCommandHandlerExecuting(IBusCommand busCommand, BrokeredMessage brokeredMessage);
        Task OnCommandHandlerSuccess(IBusCommand busCommand, BrokeredMessage brokeredMessage);
        Task OnCommandHandlerError(IBusCommand busCommand, BrokeredMessage brokeredMessage, Exception exception);
    }
}