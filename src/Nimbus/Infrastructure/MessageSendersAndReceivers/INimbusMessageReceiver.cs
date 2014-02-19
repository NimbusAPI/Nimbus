using System;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;

namespace Nimbus.Infrastructure.MessageSendersAndReceivers
{
    internal interface INimbusMessageReceiver : IDisposable
    {
        void Start(Func<BrokeredMessage, Task> callback);
        void Stop();
    }
}