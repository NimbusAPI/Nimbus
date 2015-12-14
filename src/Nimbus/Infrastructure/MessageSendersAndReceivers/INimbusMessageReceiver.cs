using System;
using System.Threading.Tasks;

namespace Nimbus.Infrastructure.MessageSendersAndReceivers
{
    public interface INimbusMessageReceiver
    {
        Task Start(Func<NimbusMessage, Task> callback);
        Task Stop();
    }
}