using System;
using System.Threading.Tasks;

namespace Nimbus.Infrastructure.MessageSendersAndReceivers
{
    internal interface INimbusMessageReceiver
    {
        Task Start(Func<NimbusMessage, Task> callback);
        Task Stop();

        Task RecordSuccess(NimbusMessage message);
        Task RecordFailure(NimbusMessage message);
    }
}