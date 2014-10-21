using Nimbus.Infrastructure.MessageSendersAndReceivers;

namespace Nimbus.Infrastructure.Heartbeat.PerformanceCounters.Custom
{
    internal class MessagesReceived : NimbusPerformanceCounterBase
    {
        public override long GetNextTransformedValue()
        {
            return GlobalMessageCounters.GetAndClearReceivedMessageCount();
        }
    }
}