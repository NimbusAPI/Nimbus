using System;
using System.Threading.Tasks;
using Nimbus.InfrastructureContracts;

namespace Nimbus.Infrastructure.MessageSendersAndReceivers
{
    internal interface INimbusMessageReceiver
    {
        Task Start(Func<NimbusMessage, Task> callback);
        Task Stop();
    }
}