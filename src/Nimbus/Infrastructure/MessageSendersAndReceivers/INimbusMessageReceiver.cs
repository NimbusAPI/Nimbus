using System;
using System.Threading.Tasks;

namespace Nimbus.Infrastructure.MessageSendersAndReceivers
{
    internal interface INimbusMessageReceiver : IDisposable
    {
        Task Start(Func<NimbusMessage, Task> callback);
        Task Stop();
    }
}