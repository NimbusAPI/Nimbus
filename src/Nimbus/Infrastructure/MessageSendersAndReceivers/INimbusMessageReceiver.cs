using System;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;

namespace Nimbus.Infrastructure.MessageSendersAndReceivers
{
    internal interface INimbusMessageReceiver : IDisposable
    {
        Task Start(Func<BrokeredMessage, Task> callback);
        Task Stop();
    }
}